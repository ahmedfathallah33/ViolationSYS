server = 'localhost:1433'  # Docker service name
database = 'Reportsdb-db'
username = 'sa'
password = 'A123456a!'
conn_str = f"mssql+pyodbc://{username}:{password}@{server}/{database}?driver=ODBC+Driver+17+for+SQL+Server"



