import websocket
import uuid
import json
import urllib.request
import urllib.parse
import argparse
import datetime
import os
from utils import *

# 프롬프트 클래스
class prompt_settings_segment:
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
        headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        }
        
        try:
            req = urllib.request.Request(
                "http://{}/prompt".format(self.args.server), 
                data=data,
                headers=headers
            )
            response = urllib.request.urlopen(req)
            return json.loads(response.read())
        except urllib.error.HTTPError as e:
            print(f"Error status: {e.code}")
            print(f"Error reason: {e.reason}")
            print(f"Error body: {e.read().decode()}")
            raise
    
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
        # Segment Image Settings
        self.workflow['10']["inputs"]["image"] = os.path.normpath(os.path.join('generated', self.args.segment_image))
        self.workflow['40']['inputs']['string'] = self.args.segment_image.split('.')[0]
        
    def run(self):
        ws = websocket.WebSocket()
        ws.connect("ws://{}/ws?clientId={}".format(self.args.server, self.client_id))
        self.set()
        images = self.get_images(ws, self.workflow)
        ws.close()

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--server', type=str, default="127.0.0.1:8000", help="Server address")
    parser.add_argument('--comfy_path', type=str, default="E:/generativeAI/ComfyUI-Desktop/ComfyUI", help="ComfyUI path")
    parser.add_argument('--output_path', type=str, default="E:/unityproject/RealTimeXGenerativeAI/GenerativeAI/Assets/StreamingAssets", help="Output path")
    parser.add_argument('--workflow', type=str, default="segment.json", help="Workflow file")
    parser.add_argument('--segment_image', type=str, default="sword_0002.png", help="Segment Generated Image file")
    
    args = parser.parse_args()

    if not os.path.exists( os.path.normpath(os.path.join(args.comfy_path, 'input/generated'))):
        os.makedirs(os.path.normpath(os.path.join(args.comfy_path, 'input/generated')))

    # Copy Segment Generated Image Unity to ComfyUI
    today = datetime.datetime.now().strftime("%Y%m%d")
    generated_img = os.path.normpath(os.path.join(args.output_path, 'GEN', today, args.segment_image))    
    target_generated = os.path.normpath(os.path.join(args.comfy_path, 'input/generated', args.segment_image))
    print(generated_img)
    print(target_generated)
    copy_images(generated_img, target_generated)

     # Merge RGB Image and Alpha Mask Image
    use_image_path = os.path.normpath(os.path.join(args.comfy_path, 'output/use'))
    if not os.path.exists(use_image_path):
        os.makedirs(use_image_path)

    # Run ComfyUI
    workflow_path = os.path.join('./workflows', args.workflow)
    with open(workflow_path, encoding='utf-8') as file:
        prompt = file.read()
    comfy_prompt = prompt_settings_segment(prompt, args)
    comfy_prompt.run()

    output_path = os.path.normpath(os.path.join(args.output_path, 'SEG', today))
    if not os.path.exists(output_path):
        os.makedirs(output_path)

    # Monitor Folder
    rgb_image_name = '{}_rgb_0001.png'.format(args.segment_image.split('.')[0])
    alpha_image_name = '{}_alpha_0001.png'.format(args.segment_image.split('.')[0])
    rgb_path = os.path.normpath(os.path.join(use_image_path, rgb_image_name))
    alpha_path = os.path.normpath(os.path.join(use_image_path, alpha_image_name))
    img_path = os.path.normpath(os.path.join(output_path, args.segment_image))
    monitor_folder(use_image_path, merge_rgb_alpha(rgb_path, alpha_path, img_path), 0.5)

if __name__ == "__main__":
    main()


