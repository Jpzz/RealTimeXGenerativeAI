import websocket
import uuid
import json
import urllib.request
import urllib.parse
import shutil
import argparse
import os

# 프롬프트 클래스
class prompt_settings:
    def __init__(self, prompt, args):
        self.prompt = prompt
        self.args = args
        self.filename = ""
        self.subfolder = ""
        self.folder_type = ""
        self.client_id = str(uuid.uuid4())

        self.workflow = None
        
    def queue_prompt(self):
        # workflow를 설정하고 전송
        self.set()  # workflow 설정
        p = {"prompt": self.workflow, "client_id": self.client_id}
        data = json.dumps(p).encode('utf-8')
        headers = {'Content-Type': 'application/json'}
        req = urllib.request.Request(
            "http://{}/prompt".format(self.args.server), 
            data=data,
            headers=headers
        )
        return json.loads(urllib.request.urlopen(req).read())
    
    def get_image(self, filename, subfolder, folder_type):
        data = {"filename": filename, "subfolder": subfolder, "type": folder_type}
        self.filename = filename
        self.subfolder = subfolder
        self.folder_type = folder_type
        url_values = urllib.parse.urlencode(data)
        with urllib.request.urlopen("http://{}/view?{}".format(self.args.server, url_values)) as response:
            return response.read()
        
    def get_history(self, prompt_id):
        with urllib.request.urlopen("http://{}/history/{}".format(self.args.server, prompt_id)) as response:
            return json.loads(response.read())

    def get_images(self, ws, prompt):
        prompt_id = self.queue_prompt()['prompt_id']
        output_images = {}
        while True:
            out = ws.recv()
            if isinstance(out, str):
                message = json.loads(out)
                if message['type'] == 'executing':
                    data = message['data']
                    if data['node'] is None and data['prompt_id'] == prompt_id:
                        break #Execution is done
            else:
                continue

        history = self.get_history(prompt_id)[prompt_id]
        for node_id in history['outputs']:
            node_output = history['outputs'][node_id]
            images_output = []
            if 'images' in node_output:
                for image in node_output['images']:
                    image_data = self.get_image(image['filename'], image['subfolder'], image['type'])
                    images_output.append(image_data)
            output_images[node_id] = images_output

        return output_images   

    def set(self):
        self.workflow = json.loads(self.prompt)
         # Set the text prompt for our positive CLIPTextEncode
        self.workflow['6']["inputs"]["text"] = self.args.positive
        # Set the text prompt for our negative CLIPTextEncode
        self.workflow['7']["inputs"]["text"] = self.args.negative
        # Set the seed for our KSampler node
        self.workflow['3']["inputs"]["seed"] = self.args.seed
        
    def run(self):
        ws = websocket.WebSocket()
        ws.connect("ws://{}/ws?clientId={}".format(self.args.server, self.client_id))
        self.set()
        images = self.get_images(ws, self.workflow)
        ws.close()

def copy_images(image_path, output_path):
    shutil.copy2(image_path, output_path)

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--server', type=str, default="127.0.0.1:8000", help="Server address")
    parser.add_argument('--positive', type=str, default="2 man", help="Positive prompt")
    parser.add_argument('--negative', type=str, default="", help="Negative prompt")
    parser.add_argument('--seed', type=int, default=1, help="Seed")
    parser.add_argument('--comfy_path', type=str, default="E:/generativeAI/ComfyUI-Desktop/ComfyUI", help="ComfyUI path")
    parser.add_argument('--output_path', type=str, default="E:/unityproject/RealTimeXGenerativeAI/GenerativeAI/Assets/StreamingAssets", help="Output path")
    parser.add_argument('--workflow', type=str, default="basicT2I.json", help="Workflow file")
    args = parser.parse_args()

    workflow_path = os.path.join('./workflows', args.workflow)
    with open(workflow_path, encoding='utf-8') as file:
        prompt = file.read()
    comfy_prompt = prompt_settings(prompt, args)
    comfy_prompt.run()

    image_path = os.path.join(args.comfy_path, comfy_prompt.folder_type, comfy_prompt.subfolder, comfy_prompt.filename)
    output_path = os.path.join(args.output_path, comfy_prompt.filename)
    print('image_path : {}'.format(image_path))
    print('output_path : {}'.format(output_path))
    copy_images(image_path, output_path)

if __name__ == "__main__":
    main()


