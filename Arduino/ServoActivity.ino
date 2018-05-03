#include <Servo.h>

int pos = 120;
int inByte = 0;
Servo servo_9;

void setup() {
  servo_9.attach(9);
  Serial.begin(9600);
}

void loop() {
  if (Serial.available()>0){
    inByte = Serial.read();
  }
  if (inByte > 200){
    for (pos = 120; pos >= 45; pos-= 1){
      servo_9.write(pos);
      delay(5);
    }
    for (pos = 45; pos <= 120; pos += 1){
      if (pos == 45){
        delay(1000);
      }
      servo_9.write(pos);
      delay(5);
    }
  }
}
