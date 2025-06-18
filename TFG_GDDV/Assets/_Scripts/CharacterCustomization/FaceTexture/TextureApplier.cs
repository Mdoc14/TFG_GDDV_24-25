using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class TextureApplier : NetworkBehaviour
{
    public Material maleFaceMaterial;
    public Material femaleFaceMaterial;
    public Material headMaterial;
    public Material earMaterial;
    public string textureFileName = "/recorteCara.png";
    private const int ChunkSize = 1000;

    private Dictionary<int, byte[]> receivedChunks = new();
    private int expectedChunks = -1;
    private byte[] lastTextureBytes = null;

    void OnEnable()
    {
        Debug.Log("[TextureApplier] OnEnable");
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    void OnDisable()
    {
        Debug.Log("[TextureApplier] OnDisable");
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[TextureApplier] OnClientConnected: {clientId}");

        if (NetworkManager.Singleton.IsClient && clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("[TextureApplier] Registrando handler de ReceiveTextureChunk en el cliente");
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("ReceiveTextureChunk", ReceiveTextureChunk);
        }

        if (IsHost && lastTextureBytes != null && clientId != NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"[TextureApplier] Cliente {clientId} se ha unido, enviando textura previamente aplicada");
            StartCoroutine(SendTextureChunksCoroutine(lastTextureBytes, clientId));
        }
    }


    // Llamado desde el botón en el host
    public void ApplyTexture()
    {
        if (!IsHost)
        {
            Debug.LogWarning("[TextureApplier] ApplyTexture llamado pero no es el host.");
            return;
        }

        Debug.Log("[TextureApplier] ApplyTexture - Host va a cargar textura");

        string fullPath = Application.streamingAssetsPath + textureFileName;
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"[TextureApplier] Archivo no encontrado: {fullPath}");
            return;
        }

        byte[] fileData = File.ReadAllBytes(fullPath);
        lastTextureBytes = fileData;


        Texture2D texture = new Texture2D(2, 2);
        if (!texture.LoadImage(fileData))
        {
            Debug.LogError("[TextureApplier] No se pudo cargar la textura desde los datos.");
            return;
        }

        Debug.Log("[TextureApplier] Textura cargada en el host, aplicando a materiales");

        if (maleFaceMaterial != null)
            maleFaceMaterial.mainTexture = texture;
        else
            Debug.LogWarning("[TextureApplier] faceMaleMaterial no está asignado (host)");

        if (femaleFaceMaterial != null)
            femaleFaceMaterial.mainTexture = texture;
        else
            Debug.LogWarning("[TextureApplier] faceFemaleMaterial no está asignado (host)");

        if (headMaterial != null)
            headMaterial.mainTexture = texture;
        else
            Debug.LogWarning("[TextureApplier] headMaterial no está asignado (host)");

        if (earMaterial != null)
            earMaterial.mainTexture = texture;
        else
            Debug.LogWarning("[TextureApplier] earMaterial no está asignado (host)");

        ulong clientId = NetworkManager.Singleton.ConnectedClients
            .FirstOrDefault(x => x.Key != NetworkManager.Singleton.LocalClientId).Key;

        Debug.Log($"[TextureApplier] Enviando textura al cliente {clientId}");

        StartCoroutine(SendTextureChunksCoroutine(fileData, clientId));
    }

    IEnumerator SendTextureChunksCoroutine(byte[] data, ulong clientId)
    {
        int totalChunks = Mathf.CeilToInt((float)data.Length / ChunkSize);
        int chunksPerFrame = 15;

        for (int i = 0; i < totalChunks; i += chunksPerFrame)
        {
            for (int j = 0; j < chunksPerFrame && i + j < totalChunks; j++)
            {
                int index = i + j;
                int offset = index * ChunkSize;
                int size = Mathf.Min(ChunkSize, data.Length - offset);
                byte[] chunk = new byte[size];
                System.Array.Copy(data, offset, chunk, 0, size);

                using var writer = new FastBufferWriter(size + 12, Allocator.Temp);
                writer.WriteValueSafe(index);
                writer.WriteValueSafe(totalChunks);
                writer.WriteValueSafe(size);
                writer.WriteBytesSafe(chunk);

                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("ReceiveTextureChunk", clientId, writer);
            }

            yield return null; 
        }
    }


    void ReceiveTextureChunk(ulong senderClientId, FastBufferReader reader)
    {
        Debug.Log($"[TextureApplier] Recibiendo chunk del cliente {senderClientId}");

        reader.ReadValueSafe(out int chunkIndex);
        reader.ReadValueSafe(out int totalChunks);
        reader.ReadValueSafe(out int chunkSize);

        if (expectedChunks == -1)
        {
            expectedChunks = totalChunks;
            Debug.Log($"[TextureApplier] Esperando {expectedChunks} chunks");
        }

        byte[] chunk = new byte[chunkSize];
        reader.ReadBytesSafe(ref chunk, chunkSize);

        receivedChunks[chunkIndex] = chunk;

        Debug.Log($"[TextureApplier] Chunk recibido {chunkIndex + 1}/{totalChunks}");

        if (receivedChunks.Count == expectedChunks)
        {
            Debug.Log("[TextureApplier] Todos los chunks recibidos, reconstruyendo textura");

            List<byte> allBytes = new();
            for (int i = 0; i < expectedChunks; i++)
                allBytes.AddRange(receivedChunks[i]);

            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(allBytes.ToArray());

            if (maleFaceMaterial != null)
                maleFaceMaterial.mainTexture = texture;
            else
                Debug.LogWarning("[TextureApplier] faceMaleMaterial no está asignado (cliente)");

            if (femaleFaceMaterial != null)
                femaleFaceMaterial.mainTexture = texture;
            else
                Debug.LogWarning("[TextureApplier] faceFemaleMaterial no está asignado (cliente)");

            if (headMaterial != null)
                headMaterial.mainTexture = texture;
            else
                Debug.LogWarning("[TextureApplier] headMaterial no está asignado (cliente)");

            if (earMaterial != null)
                earMaterial.mainTexture = texture;
            else
                Debug.LogWarning("[TextureApplier] earMaterial no está asignado (cliente)");

            receivedChunks.Clear();
            expectedChunks = -1;

            Debug.Log("[TextureApplier] Textura aplicada correctamente en cliente");
        }
    }
}
