const sql = require('mssql');
const config = {
  user: 'sa',
  password: 'A123456a!',
  server: 'localhost',  // Docker service name
  database: 'Reportsdb-db',
  options: { encrypt: false }  // For local Docker use
};