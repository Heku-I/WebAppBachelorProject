import os
import pickle

from tensorflow import keras

class IMGAccessInferencer():
    def __init__(self, tokenizer_file, model_path):
        # load tokenizer
        with open(tokenizer_file, 'rb') as f:
            self.tokenizer, self.max_len = pickle.load(f)

        # load models
        self.models = [None]*10
        for g in range(10):
            self.models[g] = keras.models.load_model(os.path.join(model_path,'model_'+str(g)),compile=False)

    def predict(self, descriptions):
        x = self.tokenizer.texts_to_sequences(descriptions)

        # pad sequences to have them all of same length
        x = keras.preprocessing.sequence.pad_sequences(x, maxlen=self.max_len, padding='post')
        y = [0]*10
        for g in range(10):
            y[g] = self.models[g].predict(x, verbose=1).flatten().tolist()

        return y

inferencer = IMGAccessInferencer('tokenizer.pkl','models')
desc = ['A man lays on a bench while his dog sits by him']
preds = inferencer.predict(desc)
print(preds)