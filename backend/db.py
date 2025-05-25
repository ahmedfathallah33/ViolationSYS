# backend/db.py
import os
import pyodbc

conn_str = (
    f"DRIVER={os.getenv('DB_DRIVER')};"
    f"SERVER={os.getenv('DB_HOST')},{os.getenv('DB_PORT')};"
    f"DATABASE={os.getenv('DB_NAME')};"
    f"UID={os.getenv('DB_USER')};"
    f"PWD={os.getenv('DB_PASSWORD')};"
    f"Encrypt={os.getenv('DB_ENCRYPT')};"
    f"Timeout={os.getenv('DB_TIMEOUT')};"
)

conn = pyodbc.connect(conn_str)