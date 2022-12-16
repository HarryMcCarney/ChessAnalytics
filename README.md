# Current Process
1. Downloaded Lichecss DB from October 2015
2. All games are extract from DB into CSV using ParsePGN.fsx
3. Player list is extracted from games csv using LichessArchive module  
4. Players rating history for each player is queried from lichess and stored in playerrating csv
5. Player ratings are averaged monthly and then rating changed since 2015 is calucalted.
6. Histgram of rating change is show and percentile distribution.

