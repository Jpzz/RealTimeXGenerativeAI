import websocket
import uuid
import json
import urllib.request
import urllib.parse
import shutil
import argparse
import os

# 프롬프트 클래스
class prompt_settings_items:
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
        # Set the seed for our KSampler node
        self.workflow['28']["inputs"]["seed"] = self.args.seed
        self.workflow['38']["inputs"]["string"] = self.args.equipments
        self.workflow['21']["inputs"]["image"] = self.args.controlnet_image
        
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
    parser.add_argument('--seed', type=int, default=1, help="Seed")
    parser.add_argument('--comfy_path', type=str, default="E:/generativeAI/ComfyUI-Desktop/ComfyUI", help="ComfyUI path")
    parser.add_argument('--output_path', type=str, default="E:/unityproject/RealTimeXGenerativeAI/GenerativeAI/Assets/StreamingAssets", help="Output path")
    parser.add_argument('--workflow', type=str, default="item.json", help="Workflow file")
    parser.add_argument('--controlnet_image', type=str, default="sword_controlnet.png", help="Controlnet image")
    parser.add_argument('--equipments', type=str, default="sword", help="Equipments")
    
    args = parser.parse_args()
    controlnet_img = os.path.normpath(os.path.join(args.output_path, 'ControlNet',args.controlnet_image))
    target_controlnet_img = os.path.normpath(os.path.join(args.comfy_path, 'input', args.controlnet_image))
    print('controlnet_img : {}'.format(controlnet_img))
    print('target_controlnet_img : {}'.format(target_controlnet_img))
    copy_images(controlnet_img, target_controlnet_img)

    workflow_path = os.path.join('./workflows', args.workflow)
    with open(workflow_path, encoding='utf-8') as file:
        prompt = file.read()
    comfy_prompt = prompt_settings_items(prompt, args)
    comfy_prompt.run()

    image_path = os.path.normpath(os.path.join(args.comfy_path, comfy_prompt.folder_type, comfy_prompt.subfolder, comfy_prompt.filename))
    output_path = os.path.normpath(os.path.join(args.output_path, comfy_prompt.filename))

    copy_images(image_path, output_path)

if __name__ == "__main__":
    main()


