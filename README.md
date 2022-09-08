# OX-210306-OTS-Q

## Control via RS terminal
XWS-30 light source using RS-485 protocol
Serial port settings: 115200 8-N-1

| Command	  | Response  |
| :---        | :---      |
| STATUS      | STATUS=x  <br> where ‘x’ is system mode:  <br> x = 0 – IDLE (ready for operations)  <br> x = 1 – STARTING (Laser is starting) <br> x = 2 – IGNITION (Plasma is triggered) <br> x = 3 – PLASMA ON (Plasma is turned ON)  <br> x = 4 – ERROR |
| REPORT      | The system gives the list of source internal parameters
| ERROR       | ERROR=xxxxx <br>Allows to know error flags if STATUS == 4 <br>0000000 means NO ERROR |
| SERIAL      | SERIAL=xxxx <br> where ‘xxxx’ is the system serial number shown as 4-digit number |
| FIRMWARE    | FIRMWARE=x.xx <br>where ‘x.xx’ is controller firmware version |
| UPTIME      | UPTIME=xxxx.x <br>where ‘xxxx.x’ is total laser/plasma active time in hours|
| PWRONTIME   | PWRONTIME=xxxx days xx hours xx minutes xx seconds <br>where ‘xxxx days xx hours xx minutes xx seconds’ is time <br>since the system was switched on|
| LASER_TEMP  | LASER_TEMP=xx.xxxx <br>where ‘xx.xxxx’ is laser module temperature, °C|
| HEAD_TEMP   | HEAD_TEMP=xx.xxxx <br>where ‘xx.xxxx’ is optical head temperature, °C|
| TEC_CUR1    | TEC_CUR1=xx.xxxx <br>where ‘xx.xxxx’ is TEC channel current in Amps|
| LASER_CUR   | LASER_CUR=xx.xxxx <br>where ‘xx.xxxx’ is laser current is Amps|
| LASER_VOL   | LASER_VOL=xx.xxxx <br>where ‘xx.xxxx’ is laser voltage in Volts|
| SUPPLY_VOL  | SUPPLY_VOL=xx.xxxx <br>where ‘xx.xxxx’ is system supply voltage in Volts |
| TURN_ON     | TURN_ON=[WAIT|OK|ERROR] <br>Turns plasma on & returns result of operation|
| TURN_OFF    | TURN_OFF=[OK|ERROR] <br>Turns plasma off & returns result of operation|
| LASER_STAT  | LASER=[ON|OFF|ERROR]|
| PLASMA_STAT | PLASMA=[ON|OFF|ERROR]|
| RESTART     | Restarts the system |
| POWEROFF    | Powers OFF the system |

---
## Category
 * PROD Info 
   * SERIAL,SERIAL=xxxx 
   * FIRMWARE,FIRMWARE=x.xx
 * Burning Info 
   * UPTIME,UPTIME=xxxx.x         //‘xxxx.x’ is total laser/plasma active time in hours
   * PWRONTIME,PWRONTIME=xxxxxx  //six digit number – time in seconds since device switched on
 * Running Info 
   * LASER_VOL,LASER_VOL=xx.xxxx       //‘xx.xxxx’is laser voltage in Volts <br>
   * LASER_CUR,LASER_CUR=xx.xxxx       //‘xx.xxxx’ is laser current is Amps <br>
   * LASER_TEMP,LASER_TEMP=xx.xxxx  //‘xx.xxxx’ is laser module temperature in Celsius <br>
   * LASER_FET_VOL ,LASER_FET_VOL=xx.xxxx   //‘xx.xxxx’is laser FET voltage drop in Volts <br>
   * SUPPLY_VOL,SUPPLY_VOL=xx.xxxx <br>
   * TEC_VOL1,TEC_VOL1=xx.xxxx <br>
   * TEC_CUR1,TEC_CUR1=xx.xxxx <br>
   * TEC_FAN1,TEC_FAN1=xxxx <br>
   * TEC_VOL2,TEC_VOL2=xx.xxxx <br>
   * TEC_CUR2,TEC_CUR2=xx.xxxx <br>
   * TEC_FAN2,TEC_FAN2=xxxx <br>
   * PHOTODIODE,PHOTODIODE=xx.xxxx  //‘xx.xxxx’ is plasma photodiode voltage in Volts
   * HEAD_TEMP,HEAD_TEMP=xx.xxxx      //‘xx.xxxx’ is optical head temperature in Celsius 
 * State Info 
   * PLASMA_STAT,PLASMA=[ON|OFF|ERROR]
   * LASER_STAT,LASER=[ON|OFF|ERROR] 
 * Operations 
    * TURN_ON  (plasma)
    * TURN_OFF  (plasma)
    * RESTART     (system)
    * POWEROFF (system)

## Operation FSM Overview
![FSM overview](http://www.plantuml.com/plantuml/svg/dPJVJzim4CVVyrVSG09fek87FA4sec8jgWH9bHGnQKp8nDTWpRMPusn2PF_xujWHSJ5xcDLAzjrzlcUVpxceD56wtl18CyqHljxb89zH4SsaW7b-4pa7HEDTpied_SXhFDXLIEDc0f2lfglR7C3CAMh2k9rVoiq6XQwWwRbI8Ogu0JXg8m1WyULwPcOA2Ns1jLHlUKFKiQNQVDbgaLoPrJMfK06he3i34sK0Et79hfB5Qf4cPhNafDeGmsd5oX8LqW1uQi7bzJI_cS8xibO2QMiEa6UMd6LPchNJyMv2Vht4uq9ogSvJHKI96T9u_1X3SZXj8XGKkXf74S2tpzzXD8fX8PXUIgLRKp_jx8KK0Wjps0W6YoqVd8FBP-vfnJOeQxq5q9QuVCBYHww9HZWV02PR7uiqWc7SoCFyHC-ecAIi89o_lATKBOgsAuYWFdKfXLQIGuO_Qwnq1CF8KUtbsrAjmwVkPSMSC8uK8Z2_oZ13xTVM-5RnL80TdWhih6wpv25DO6_lNjqBpsURVp8vsl6Lrkj_ACtd8IdRPmz6SJ994rymmwx3jmIpMVSOm0mlV2b_evBhjH--JEzcMPlTXBfQfCBMm8pSlI7RVQLWvY7spdvjdEuLFAJ2Plqp56oJlZgh-KVECRoqLsFjTslEHgXnr1Kun-6-5NOeMyjd3UDnyIHPWN5iVTOwghtx3zUdQUvLg12U4FLsuWiXhn1izFvXP7SvZ1vHU-cS2GnSlBRFBumjA3TC0Ek-7srl1Rh5zTtH1v9YgMI1LVKVaiS79mUdOTcPKb9zlDDFRx9EwwnhWGiKjDxmlm00)

[Definition of Diagram](https://github.com/williamzhou-sioux/OX-210306-OTS-Q/blob/master/assets/fsm_overview.plantuml)


