import firebase_admin
from firebase_admin import credentials


def InitializeFirebase():
    cred = credentials.Certificate("res/serviceAccountKey.json")
    firebase_admin.initialize_app(cred,{ 'databaseURL': "https://words-unity-36083-default-rtdb.europe-west1.firebasedatabase.app"})
