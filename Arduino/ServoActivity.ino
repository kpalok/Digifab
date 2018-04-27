#include <Servo.h>

int pos = 0;
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
    for (pos = 0; pos <= 45; pos+= 1){
      servo_6.write(pos);
      delay(15);
    }
    for (pos = 45; pos >= 0; pos -= 1){
      servo_6.write(pos);
      delay(15);
    }
  }
}
