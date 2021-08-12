cd /d %~dp0

psftp safv_ncbs -pw nawadata -b transfer.txt < yes.txt  
pause