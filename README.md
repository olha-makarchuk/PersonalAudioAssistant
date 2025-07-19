# 🎧 Personal Audio Assistant

A cross-platform personal voice assistant that processes user voice input via advanced audio analysis, speaker diarization, and transcription. The system is built using a modular, scalable architecture that separates client-side logic, backend processing, and AI-driven services.

---

## ✅ Future Enhancements

- **🖥️ Real-time Streaming Transcription**  
  Display live transcription during user interaction to improve clarity and responsiveness.

- **🗣️ Enhanced Speaker Identification & Separation**  
  Improve voiceprint matching in noisy environments and among users with similar voices through hybrid diarization models.

- **🧬 Personalized Multi-Profile Voice Support**  
  Expand customizable voice profiles with user-defined parameters within a single account.

- **🎧 Continuous Listening & Full Voice Interaction Cycle**  
  Support a complete voice interaction cycle with always-on listening, wake-word detection, command recognition, and response playback for seamless hands-free experience.

- **💳 Flexible Token-Based Billing System**  
  Enable usage-based pricing models (based on duration, transcription tokens, or API consumption) with custom limits and top-ups.

- **🧠 Voice Cloning Enhancements**  
  Improve cloned voice fidelity and allow user-tuned adjustments (e.g., emotion overlay, speed, pitch).

- **📜 Voice Interaction History**  
  Enable searchable, timestamped message history with built-in audio replay and filtering by speaker or topic.

- **📊 In-App Voice Analytics & Usage Tracking**  
  Add visual statistics within the Android app to show session history, profile activity, and token consumption.

---
## 🧩 Repository Structure

- **`ApiResonalAudioAssistant/`** – .NET Core backend using **Onion architecture** and **CQRS** pattern.
- **`PersonalAudioAssistant/`** – .NET MAUI cross-platform client application following the **MVVM** (Model–View–ViewModel) pattern.
- **`Server/`** – Python microservice (FastAPI) handling audio stream processing, speaker diarization, and voice validation.


---

## 🧠 Client – .NET MAUI Application

- **Architecture:** MVVM with clean separation across `Contracts/`, `Application/`, and `ViewModels/`.
- **Features:**
  - Reactive two-way data binding via `INotifyPropertyChanged` & `ICommand`.
  - Local caching to reduce redundant API calls and improve performance.
- **Key Libraries:**
  - `CommunityToolkit.Maui`
  - `Microcharts.Maui`
  - `Syncfusion.Maui.Popup`
  - `Plugin.Maui.Audio`

---

## 🧱 Backend – .NET Core API

- **Architectural Patterns:**
  - Onion Architecture
  - CQRS with `MediatR`
- **Project Layers:**
  - `Presentation`: REST controllers
  - `Application`: Business logic via MediatR
  - `Domain`: Entities, Value Objects, Domain Events
  - `Infrastructure`: Data access, external integrations
- **Technologies Used:**
  - `Entity Framework Core 8`
  - `AutoMapper`
  - `MediatR`
  - `Swagger` via `Swashbuckle.AspNetCore`
  - **Authentication:**
    - Google OAuth2
    - JWT (`Microsoft.IdentityModel.Tokens`)
  - **Cloud Services:**
    - Azure Blob Storage (for audio & data files)
    - Azure Cosmos DB (for scalable NoSQL data)
    - Google Drive API (for audio backup & retrieval)

---

## 🎙️ Python Microservice – Audio Intelligence (FastAPI)

Handles real-time audio processing, diarization, transcription, and voiceprint generation.

- **Endpoints:**
  - `POST /ws` – WebSocket for real-time audio input
  - `POST /embedding` – Voiceprint vector generation
  - `POST /tokenizer` – Token counting for NLP usage
- **Pipeline:**
  - Detect input mode (timeout/first phrase/end phrase)
  - Context-based waiting logic
  - Speaker diarization + voiceprint matching
  - Audio segmentation, merging, transcription
- **Libraries & Tools:**
  - FastAPI, Uvicorn, Gunicorn
  - `openai-whisper`, `Vosk`, `pyannote.audio`, `SpeechRecognition`
  - `Resemblyzer`, `webrtcvad`, `torchaudio`, `librosa`

---

## 🤖 AI & External APIs

### ✨ AI Models & Tools

- **Transcription & ASR:**
  - `openai/whisper`
  - `Vosk`
- **Speaker Embeddings & Diarization:**
  - `speechbrain/spkrec-ecapa-voxceleb`
  - `nvidia/diar_sortformer_4spk-v1`
  - `pyannote.audio`

### 🌐 External APIs

- **OpenAI API** – For transcription, NLP tasks, and token usage tracking
- **ElevenLabs API** – For high-quality text-to-speech synthesis and voice cloning
- **Google OAuth2** – For secure user authentication and access delegation
- **Azure Services:**
  - **Blob Storage** – Cloud storage for audio and metadata
  - **Cosmos DB** – NoSQL database for backend data persistence

---

## 📦 Dependencies

| Layer | Path | Description |
|-------|------|-------------|
| 🔧 Python Microservice | `Server/requirements.txt` | All Python libraries for audio & AI |
| 🔩 .NET Backend | `ApiResonalAudioAssistant/dependencies.txt` | All NuGet packages used in backend |
| 📱 .NET MAUI Client | `PersonalAudioAssistant/dependencies.txt` | All UI/logic libraries for client |
