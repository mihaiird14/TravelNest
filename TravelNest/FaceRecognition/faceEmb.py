from flask import Flask, request, jsonify
import face_recognition
app=Flask(__name__)
@app.route('/faceEmb',methods=['POST'])
def emb():
    date=request.json
    imaginePath=date['image_path']
    imagine=face_recognition.load_image_file(imaginePath)
    embeddings=face_recognition.face_encodings(imagine)
    if len(embeddings)==0:
        return jsonify({"faces":[]})
    faces=[enc.tolist() for enc in embeddings]
    return jsonify({"faces":faces})

if __name__=='__main__':
    app.run(port=5001)