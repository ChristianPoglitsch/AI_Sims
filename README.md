### SimVille



We present SimVille, a mixed reality game that combines extended reality (XR), large language models (LLMs), and conversational non-player characters (NPCs) to explore new forms of immersive, socially interactive learning. In SimVille, players engage in quests by conversing naturally with embodied NPCs, where dialogue is dynamically generated through LLMs. The game leverages XR to situate interactions in a shared physical-digital environment, enabling players to move, explore, and connect with NPCs in contextually rich scenarios. Beyond quest completion, the system investigates how adaptive dialogue, social cues, and narrative-driven embodiment can support the development of communication skills, such as small talk, irony detection, or emotion recognition. We describe the design of SimVille, including technical integration of XR and LLM pipelines, interaction mechanics, and a framework for evaluating conversational experiences. Our contribution highlights opportunities and challenges in creating explainable, personalized, and engaging human–AI interactions in augmented and mixed reality.







#### Sample Scene

-> Scene/SampleScene



Before you start

GameState: Change between TTS/STT and chat based system

Camera: Change between Desktop and XR camera



#### Simple Chat

-> Scene/LLM\_Chat

### 

## Assets





###### Text2Speech (TTS) / Speech2Text (STT)



⦁	Requires an OpenAI API Key, or

⦁	Use a local Whisper server -> \[tts\_server.py](https://github.com/ChristianPoglitsch/AIAgents -> TTS\_STT/tts\_server.py)



###### Avatars / Characters

[Integrating Ready Player Me characters into Unity](https://readyplayer.me/blog/integrating-ready-player-me-characters-into-diverse-game-art-styles-demo-using-shaders-in-unity)

###### Large Language Model

[LLM for Unity (Unity Asset Store)](https://assetstore.unity.com/packages/tools/ai-ml-integration/llm-for-unity-273604)



📄 Read the Paper: \[Download SimVille.pdf](https://github.com/ChristianPoglitsch/AI\_Sims/raw/main/SimVille.pdf)





---



\## 🧰 System Requirements



\### General

\- \*\*Memory:\*\* 16 GB (minimum)



\### 🧠 Large Language Models (LLMs)

> ⚙️ \*GPU strongly recommended for performance\*



| Model   | GPU Memory Requirement |

|----------|------------------------|

| \*\*Mistral\*\* | ~6 GB VRAM |

| \*\*Gemini\*\*  | ~12 GB VRAM |



\### 🎙️ Whisper (Speech-to-Text)

> 🖥️ \*GPU recommended\*

\- \*\*Memory:\*\* ~500 MB VRAM



---



\## 🚀 Notes

\- LLMs can run on CPU, but performance will be significantly slower.

\- For best results, use a GPU with CUDA support (e.g., NVIDIA RTX series).

\- Check `requirements.txt` for Python dependencies.



