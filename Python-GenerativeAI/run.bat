@echo off
call genAI\Scripts\activate.bat
set server="127.0.0.1:8000"
set positive="1 girl, on the beach, beautiful sky, afternoon, cinematic lighting, master piece"
set negative="worst quality"
set seed=31690
set output_path="E:/unityproject/Generative_AI_Pipeline/GenerativeAI/Assets/StreamingAssets"
set comfy_path="E:/generativeAI/ComfyUI-Desktop/ComfyUI"
set workflow = "bacicT2I.json"
python websocket_api.py --server %server% --positive %positive% --negative %negative% --seed %seed% --comfy_path %comfy_path% --output_path %output_path% --basicT2I %workflow%