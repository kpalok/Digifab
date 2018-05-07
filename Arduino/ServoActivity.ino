#include <Servo.h>

int pos = 120;
int inByte = 0;
Servo servo_6;

void setup() {
  servo_6.attach(6);
  Serial.begin(9600);
}

void loop() {
  if (Serial.available()>0){
    inByte = Serial.read();
  }
  if (inByte > 200){
    for (pos = 120; pos >= 45; pos-= 1){
      servo_6.write(pos);
      delay(5);
    }
    for (pos = 45; pos <= 120; pos += 1){
      if (pos == 45){
        delay(300);
      }
      servo_6.write(pos);
      delay(5);
    }
  }
  else servo_6.write(120);
}
