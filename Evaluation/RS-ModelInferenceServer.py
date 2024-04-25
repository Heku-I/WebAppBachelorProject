import os
import pickle
from flask import Flask, request, jsonify
from tensorflow import keras

app = Flask(__name__)

class IMGAccessInferencer:
    def __init__(self, tokenizer_file, model_path):
        # load tokenizer
        with open(tokenizer_file, 'rb') as f:
            self.tokenizer, self.max_len = pickle.load(f)

        # load models
        self.models = [None] * 10
        for g in range(10):
            self.models[g] = keras.models.load_model(os.path.join(model_path, 'model_' + str(g)), compile=False)

    def predict(self, descriptions):
        x = self.tokenizer.texts_to_sequences(descriptions)

        # pad sequences to have them all of the same length
        x = keras.preprocessing.sequence.pad_sequences(x, maxlen=self.max_len, padding='post')
        y = [0] * 10
        for g in range(10):
            y[g] = self.models[g].predict(x, verbose=0).flatten().tolist()  # Set verbose to 0 to avoid too much logging on server

        return y

# Initialize the inferencer
inferencer = IMGAccessInferencer('tokenizer.pkl', 'models')

@app.route('/predict', methods=['POST'])
def predict():
    # Get description from the request
    data = request.get_json()
    description = data.get('description', "")

    if not description:
        return jsonify({'error': 'No description provided'}), 400

    # Make prediction
    desc = [description]
    preds = inferencer.predict(desc)

    # Return predictions
    return jsonify({'predictions': preds})

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5005)