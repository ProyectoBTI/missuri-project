
using System;
using System.IO;
using System.Linq; // Para usar Contains
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Networking;
using TMPro;
using HuggingFace.API;
using Newtonsoft.Json;

public class SpeechRecognitionTest : MonoBehaviour {
    [Header("UI Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private TextMeshProUGUI text;

    [Header("Voice Configuration")]
    [SerializeField] private string elevenLabsVoiceId = "9BWtsMINqrJLrRacOk9x";
    [SerializeField] private string elevenLabsApiKey = "sk_3d596b340d603183460cb2c92c98d582dfeba72d2a0b5626";
    [SerializeField] private string elevenLabsApiUrl = "https://api.elevenlabs.io";

    [Header("Google Gemini Configuration")]
    private string gasURL = "https://script.google.com/macros/s/AKfycbxep2QniTva_C9cDvfdqi4BwROVOYzgtRl2ro1QWa-ObqB7_2g33AnjmowEQJR4R25WyA/exec";

    private AudioClip clip;
    private byte[] bytes;
    private bool recording;
    private string selectedMic;
    public UnityEvent<AudioClip> AudioReceived;

    private void Start() {
        // Listar todos los micrófonos detectados
        foreach (var device in Microphone.devices) {
            Debug.Log("Micrófono detectado: " + device);
        }

        // Configurar el micrófono que queremos usar
        selectedMic = "External Mic (Realtek(R) Audio)";
        if (Microphone.devices.Contains(selectedMic)) {
            Debug.Log($"Usando micrófono: {selectedMic}");
        } else {
            Debug.LogError("Micrófono no encontrado o no disponible.");
            text.text = "Micrófono no encontrado.";
            return; // No continuar si no se encuentra el micrófono
        }

        startButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);
    }

    private void Update() {
        if (recording) {
            var position = Microphone.GetPosition(selectedMic);
            Debug.Log($"Posición actual del micrófono: {position}");
            
            // Detener la grabación si ha alcanzado la longitud máxima
            if (position >= clip.samples) {
                StopRecording();
            }
        }
    }

    private void StartRecording() {
        if (selectedMic == null || !Microphone.devices.Contains(selectedMic)) {
            text.text = "Micrófono no disponible.";
            return;
        }

        // Asegurarse de que no haya grabación previa
        if (Microphone.IsRecording(selectedMic)) {
            Microphone.End(selectedMic);
        }

        // Iniciar grabación
        clip = Microphone.Start(selectedMic, false, 10, 44100);
        if (clip == null) {
            text.text = "No se pudo iniciar la grabación.";
            Debug.LogError("Error al iniciar la grabación con el micrófono: " + selectedMic);
            return;
        }

        recording = true;
        text.text = "Recording started...";
        Debug.Log("Grabación iniciada.");
    }

    private void StopRecording() {
        if (selectedMic == null || !Microphone.IsRecording(selectedMic)) {
            text.text = "No se estaba grabando.";
            return;
        }

        var position = Microphone.GetPosition(selectedMic);
        Microphone.End(selectedMic);

        if (position <= 0) {
            text.text = "No audio data recorded.";
            Debug.LogWarning("No se capturó audio del micrófono.");
            return;
        }

        // Extraer los datos de audio grabados
        var samples = new float[position * clip.channels];
        clip.GetData(samples, 0);
        bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);

        // Guardar el archivo WAV
        File.WriteAllBytes(Application.dataPath + "/test.wav", bytes);
        text.text = "Recording stopped. File saved.";
        Debug.Log("Grabación detenida. Archivo guardado en test.wav.");
        SendRecording();

        recording = false;
    }

    private void SendRecording() {
    HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response => {
        text.color = Color.white;
        text.text = response;
        StartCoroutine(SendDataToAPI(response));
    }, error => {
        text.color = Color.red;
        text.text = error;
    });
}

    private IEnumerator SendDataToAPI(string recognizedText)
    {
        // Preparar el formulario de datos
        WWWForm form = new WWWForm();
        form.AddField("parameter", recognizedText);

        // Crear la solicitud POST
        UnityWebRequest www = UnityWebRequest.Post(gasURL, form);

        Debug.Log($"Enviando solicitud a: {gasURL}");
        Debug.Log($"Texto reconocido: {recognizedText}");

        // Enviar la solicitud y esperar la respuesta
        yield return www.SendWebRequest();

        // Manejo de la respuesta
        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Respuesta recibida: {www.downloadHandler.text}");
        }
        else
        {
            Debug.LogError($"Error al enviar datos: {www.error}");
            if (!string.IsNullOrEmpty(www.downloadHandler.text))
            {
                Debug.LogError($"Respuesta del servidor: {www.downloadHandler.text}");
            }
        }

        string geminiResponse = www.downloadHandler.text;

        // Convertir texto a voz con Eleven Labs
        StartCoroutine(GetAudioFromElevenLabs(geminiResponse));
    }

    private IEnumerator GetAudioFromElevenLabs(string textToSpeak) {
        var postData = new ElevenLabsRequest {
            text = textToSpeak,
            model_id = "eleven_monolingual_v1",
            voice_settings = new VoiceSettings {
                stability = 0,
                similarity_boost = 0,
                style = 0.5f,
                use_speaker_boost = true
            }
        };

        string json = JsonConvert.SerializeObject(postData);
        string url = $"{elevenLabsApiUrl}/v1/text-to-speech/{elevenLabsVoiceId}";
        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, json);
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("xi-api-key", elevenLabsApiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error al obtener audio de Eleven Labs: " + request.error);
            text.text = "Error al convertir texto en voz.";
            yield break;
        }

        AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);
        AudioReceived?.Invoke(audioClip);

        // Reproducir el audio
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.Play();
        text.text = "Reproduciendo respuesta...";
    }

    

    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels) {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2)) {
            using (var writer = new BinaryWriter(memoryStream)) {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples) {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }

    [Serializable]
    public class ElevenLabsRequest {
        public string text;
        public string model_id;
        public VoiceSettings voice_settings;
    }
    [Serializable]
    public class VoiceSettings {
        public int stability;
        public int similarity_boost;
        public float style;
        public bool use_speaker_boost;
    }
}
