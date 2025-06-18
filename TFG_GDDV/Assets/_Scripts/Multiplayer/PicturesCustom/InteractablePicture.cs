using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class InteractablePicture : NetworkBehaviour, IInteractable
{
    [Header("Nombre del archivo (sin extensión) que se usará para guardar la textura")]
    public string textureFileName;
    private string handlerName;

    [Header("Referencia al ImageFit que aplica la textura al plano")]
    public ImageFit imageFit;

    private const int ChunkSize = 1400;

    private byte[] localImageData;
    private readonly Dictionary<int, byte[]> receivedChunks = new();
    private int expectedChunks = -1;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[Cliente {OwnerClientId}] OnNetworkSpawn - IsClient: {IsClient}, IsServer: {IsServer}");

        handlerName = $"ReceiveTextureChunk_{textureFileName}";
        if (IsClient)
        {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(handlerName, OnReceiveChunk);
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    public void Interact()
    {
        if (!IsHost) return;

        string path = LoadFileScript.PickImagePath();
        if (string.IsNullOrEmpty(path)) return;

        localImageData = File.ReadAllBytes(path);

        // Convertir en textura
        Texture2D tex = new Texture2D(2, 2);
        if (!tex.LoadImage(localImageData))
        {
            Debug.LogError("[Host] No se pudo cargar la imagen como textura.");
            return;
        }

        // Guardar en StreamingAssets
        string savePath = Application.streamingAssetsPath + "/" + textureFileName + ".png";
        File.WriteAllBytes(savePath, tex.EncodeToPNG());

        // Aplicar localmente
        imageFit.texture = LoadTextureFromStreamingAssets();
        imageFit.ApplyImage();

        // Enviar a los clientes
        Debug.Log($"[Host] Clientes conectados: {string.Join(", ", NetworkManager.Singleton.ConnectedClientsIds)}");
        foreach (var client in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (client != NetworkManager.Singleton.LocalClientId)
                Debug.Log($"[Host] Enviando textura a cliente {client}: {localImageData}");
            StartCoroutine(SendChunks(client, localImageData));
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsHost && localImageData != null)
        {
            Debug.Log($"[Host] Cliente {clientId} conectado. Reenviando textura...");
            StartCoroutine(SendChunks(clientId, localImageData));
        }
    }

    private IEnumerator SendChunks(ulong clientId, byte[] data)
    {
        int totalChunks = Mathf.CeilToInt((float)data.Length / ChunkSize);
        int chunksPerFrame = 10;

        for (int i = 0; i < totalChunks; i += chunksPerFrame)
        {
            for (int j = 0; j < chunksPerFrame && (i + j) < totalChunks; j++)
            {
                int index = i + j;
                int offset = index * ChunkSize;
                int size = Mathf.Min(ChunkSize, data.Length - offset);

                int totalSize =
                    FastBufferWriter.GetWriteSize(index) +
                    FastBufferWriter.GetWriteSize(totalChunks) +
                    FastBufferWriter.GetWriteSize(size) +
                    FastBufferWriter.GetWriteSize(textureFileName) +
                    size;

                Debug.Log($"[Debug] Chunk {index}, chunk data size: {size}, total writer size: {totalSize}");
                using var writer = new FastBufferWriter(totalSize, Allocator.Temp);
                Debug.Log($"[Debug] Writer capacity: {writer.Capacity}");
                writer.WriteValueSafe(index);
                writer.WriteValueSafe(totalChunks);
                writer.WriteValueSafe(size);
                writer.WriteValueSafe(textureFileName);

                byte[] chunkData = new byte[size];
                System.Array.Copy(data, offset, chunkData, 0, size);
                writer.WriteBytesSafe(chunkData, size);


                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(handlerName, clientId, writer, NetworkDelivery.ReliableFragmentedSequenced);
            }

            yield return null;
        }

        Debug.Log($"[Host] Textura enviada completamente al cliente {clientId}");
    }


    private void OnReceiveChunk(ulong senderClientId, FastBufferReader reader)
    {
        Debug.Log($"[Cliente {NetworkManager.Singleton.LocalClientId}] Recibió chunk del servidor.");
        reader.ReadValueSafe(out int index);
        reader.ReadValueSafe(out int total);
        reader.ReadValueSafe(out int size);
        reader.ReadValueSafe(out string fileName);

        if (expectedChunks == -1)
        {
            expectedChunks = total;
            receivedChunks.Clear();
            textureFileName = fileName; // Asignar nombre desde el mensaje
            Debug.Log($"[Cliente] Iniciando recepción de {total} chunks para {fileName}");
        }

        byte[] chunk = new byte[size];
        reader.ReadBytesSafe(ref chunk, size);
        receivedChunks[index] = chunk;

        if (receivedChunks.Count == expectedChunks)
        {
            Debug.Log("[Cliente] Todos los chunks recibidos. Reconstruyendo imagen...");

            List<byte> imageData = new();
            for (int i = 0; i < expectedChunks; i++)
            {
                if (!receivedChunks.TryGetValue(i, out var part))
                {
                    Debug.LogError($"[Cliente] Falta chunk {i}");
                    return;
                }
                imageData.AddRange(part);
            }

            // Guardar en StreamingAssets
            string savePath = Application.streamingAssetsPath + "/" + textureFileName + ".png";
            File.WriteAllBytes(savePath, imageData.ToArray());

            // Aplicar
            imageFit.texture = LoadTextureFromStreamingAssets();
            imageFit.ApplyImage();

            expectedChunks = -1;
            receivedChunks.Clear();
        }
    }

    private Texture2D LoadTextureFromStreamingAssets()
    {
        string path = Application.streamingAssetsPath + "/" + textureFileName + ".png";
        if (!File.Exists(path))
        {
            Debug.LogError("[StreamingAssets] No se encontró el archivo: " + path);
            return null;
        }

        byte[] data = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(data);
        return tex;
    }
}
