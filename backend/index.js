const sql = require('mssql');

const config = {
  user: process.env.DB_USER,
  password: process.env.DB_PASSWORD,
  server: process.env.DB_HOST, // This will be 'mssql' from docker-compose
  database: process.env.DB_NAME,
  options: {
    encrypt: true,
    trustServerCertificate: true // For development only
  }
};

async function connectToDatabase() {
  try {
    await sql.connect(config);
    console.log('Connected to SQL Server');
    
    // Create database if it doesn't exist
    await sql.query`IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'reportsdb') CREATE DATABASE reportsdb`;
    
    // Switch to your database
    config.database = 'reportsdb';
    await sql.connect(config);
    
  } catch (err) {
    console.error('Database connection failed:', err);
  }
}

connectToDatabase();