---
title: "Week Eight"
---

[Home](https://kpalok.github.io/Digifab/)

# Week Eight

The bluetooth application is now working:

![Image missing](https://raw.githubusercontent.com/kpalok/Digifab/gh-pages/Images/BluetoothDemo.gif)

We decided to make a PCB that can mounted on top of the arduino. We made the design with Eagle's library by using standard pin header parts for the
holes. I (Joona) should have been more careful at the design part because I made a mistake with the width of the pins so some of them 
didn’t fit correctly. We later learned that arduino has its own library for Eagle and it would have had the right components for perfect
fit. The drilling process also didn’t go as planned. Both of the mills did the drilling deeper on the other side of the board. The first
attempt was a failure but the second one did work although some of the lines were extremely thin. After the board was done we noticed the
mistake with the pin width and also the data pin from the servo was routed to wrong place. We fixed the issue by soldering a piece of 
metal from another pin to the wrongly routed data pin.

![Image missing](https://github.com/kpalok/Digifab/blob/gh-pages/Images/DoorDemo.gif)



[Week 7](https://kpalok.github.io/Digifab/2018/04/25/weekly-report.html)
