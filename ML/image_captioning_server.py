
from flask import Flask, request, jsonify
import numpy as np
import pickle
from tensorflow.keras.applications.vgg16 import VGG16, preprocess_input
from tensorflow.keras.preprocessing.image import load_img, img_to_array
from tensorflow.keras.preprocessing.sequence import pad_sequences
from tensorflow.keras.models import load_model
from tensorflow.keras.models import Model
from io import BytesIO

app = Flask(__name__)

# Load models and tokenizer
vgg_model = VGG16()
# Reconfigure to use the output from the second-to-last layer
vgg_model = Model(inputs=vgg_model.inputs, outputs=vgg_model.layers[-2].output)

model = load_model('test_model.h5')
with open('tokenizer.pkl', 'rb') as f:
    tokenizer = pickle.load(f)
max_length = 35

def idx_to_word(integer, tokenizer):
    for word, index in tokenizer.word_index.items():
        if index == integer:
            return word
    return None

def predict_caption(model, image, tokenizer, max_length):
    in_text = 'startseq'
    for i in range(max_length):
        sequence = tokenizer.texts_to_sequences([in_text])[0]
        sequence = pad_sequences([sequence], maxlen=max_length)
        yhat = model.predict([image, sequence], verbose=0)
        yhat = np.argmax(yhat)
        word = idx_to_word(yhat, tokenizer)
        if word is None:
            break
        in_text += ' ' + word
        if word == 'endseq':
            break
    return in_text.replace('startseq', '').replace('endseq', '')

@app.route('/predict', methods=['POST'])
def upload_file():
    if 'image' not in request.files:
        return jsonify({'error': 'No image part'})
    file = request.files['image']
    if file.filename == '':
        return jsonify({'error': 'No selected file'})
    if file:
        image_stream = BytesIO()
        file.save(image_stream)
        image_stream.seek(0)
        image = load_img(image_stream, target_size=(224, 224))
        image = img_to_array(image)
        image = np.expand_dims(image, axis=0)
        image = preprocess_input(image)
        features = vgg_model.predict(image)
        caption = predict_caption(model, features, tokenizer, max_length)
        print("Predicted caption: " + caption)
        return jsonify({'caption': caption})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
