import requests

def send_image_to_server(image_path, server_url):
    """Send an image to the server and print the received caption."""
    with open(image_path, 'rb') as file:
        files = {'image': file}
        response = requests.post(server_url, files=files)
        if response.status_code == 200:
            print("Received response:", response.json())
        else:
            print("Failed to get a response, status code:", response.status_code)

# URL of the server endpoint
server_url = 'http://localhost:5000/predict'

# Path to your image file
image_path = 'images/kitty.jpg'

# Send the image to the server
send_image_to_server(image_path, server_url)