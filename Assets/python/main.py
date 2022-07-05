import cv2 as cv
import win32pipe, win32file
import json
class PipeServer():
    def __init__(self, pipeName):
        self.pipe = win32pipe.CreateNamedPipe(
        r'\\.\pipe\\'+pipeName,
        win32pipe.PIPE_ACCESS_OUTBOUND,
        win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_READMODE_MESSAGE | win32pipe.PIPE_WAIT,
        1, 65536, 65536,
        0,
        None)
    
    #Carefull, this blocks until a connection is established
    def connect(self):
        win32pipe.ConnectNamedPipe(self.pipe, None)
    
    #Message without tailing '\n'
    def write(self, message):
        win32file.WriteFile(self.pipe, message.encode()+b'\n')

    def close(self):
        win32file.CloseHandle(self.pipe)

###############################################################################################################

print("Running")
capture = cv.VideoCapture(0) #to open Camera
print("Created capture instance")
pretrained_model = cv.CascadeClassifier("python/face_detector.xml") 
#accessing pretrained model
print("loaded model")

t = PipeServer("FaceRecogServer")
print("instantiated pipeline")
print("connecting to pipeline")
t.connect()
print("connected to pipeline")

while True:
    boolean, frame = capture.read()
    if boolean == True:
        gray = cv.cvtColor(frame, cv.COLOR_BGR2GRAY)
        coordinate_list = pretrained_model.detectMultiScale(gray, scaleFactor=1.1, minNeighbors=3) 
        
        # drawing rectangle in frame
        for (x,y,w,h) in coordinate_list:
            cv.rectangle(frame, (x,y), (x+w, y+h), (0,255,0), 2)
            
            x = 500-x
            y = 500-y
            
            t.write(f"{x},{y},{w},{h}")
            
            # print(f"x: {x}; y: {y}; w: {w}; h: {h}")
            
        # Display detected face
        cv.imshow("Live Face Detection", frame)
        
        # condition to break out of while loop
        if cv.waitKey(20) == ord('x'):
            break
        
capture.release()
print("ended capture")
cv.destroyAllWindows()
print("closed window")
t.close()
print("closed pipeline")