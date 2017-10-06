using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SqlServerFilestreamTesting
{
    public class FileStreamManager
    {
        /* Reference: https://www.codeproject.com/Articles/128657/How-Do-I-Use-SQL-File-Stream */

        public void WriteFileToFileStream(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                this.WriteFileToFileStream(fs);
            }
        }
        public void WriteFileToFileStream(Stream stream)
        {
            using (TransactionScope transactionScope = new TransactionScope())
            {
                SqlConnection sqlConnection1 = new SqlConnection(conn);
                SqlCommand sqlCommand1 = sqlConnection1.CreateCommand();
                string insertCommand = "Insert Into PictureTable(Description, FileData) values('" + Guid.NewGuid().ToString() + "',Cast('' As varbinary(Max)));",
                        selectCommand = "Select FileData.PathName() As Path From PictureTable Where PkId =@@Identity";

                //First create an empty file to have the path and then select this path
                sqlCommand1.CommandText = insertCommand + selectCommand;
                sqlConnection1.Open();
                string filePath1 = (string)sqlCommand1.ExecuteScalar();

                SqlConnection sqlConnection2 = new SqlConnection(conn);
                SqlCommand sqlCommand2 = sqlConnection2.CreateCommand();
                sqlCommand2.CommandText = "Select GET_FILESTREAM_TRANSACTION_CONTEXT() As TransactionContext";
                sqlConnection2.Open();

                // Prepare the filestream (file) for write
                byte[] transactionContext1 = (byte[])sqlCommand2.ExecuteScalar();
                SqlFileStream sqlFileStream1 = new SqlFileStream(filePath1, transactionContext1, FileAccess.Write);

                // Open the source file and read its content
                this.WriteToFile(stream, 5, sqlFileStream1);
                
                sqlFileStream1.Close();
                transactionScope.Complete();
            }
        }

        public byte[] ReadFileFromFileStream(int id)
        {
            using (TransactionScope transactionScope2 = new TransactionScope())
            {
                SqlConnection sqlConnection3 = new SqlConnection(conn);
                SqlCommand sqlCommand3 = sqlConnection3.CreateCommand();
                sqlCommand3.CommandText = "Select FileData.PathName() As Path, GET_FILESTREAM_TRANSACTION_CONTEXT() As TransactionContext From PictureTable Where PkId = @PkId";

                sqlCommand3.Parameters.Add("@PkId", SqlDbType.Int).Value = id;

                sqlConnection3.Open();

                SqlDataReader reader = sqlCommand3.ExecuteReader();
                reader.Read();

                string filePath = (string)reader["Path"];

                byte[] transactionContext2 = (byte[])reader["TransactionContext"];

                int bufferLength = 5;
                using (SqlFileStream sqlFileStream2 = new SqlFileStream(filePath, transactionContext2, FileAccess.Read))
                {
                    List<byte> result = new List<byte>();
                    while(true)
                    {
                        long tillStreamEnd = sqlFileStream2.Length - sqlFileStream2.Position;
                        if (tillStreamEnd == 0)
                        {
                            break;
                        }
                        if (bufferLength > tillStreamEnd)
                        {
                            bufferLength = (int)tillStreamEnd;
                        }

                        byte[] data = new byte[bufferLength];
                        sqlFileStream2.Read(data, 0, bufferLength);
                        result.AddRange(data);
                    }

                    return result.ToArray();
                }
            }
        }

        #region Private members
        private const string conn = @"Data Source=.\MSSQLSERVER2016;Initial Catalog = FilestreamTesting;Integrated Security = True";
        private void WriteToFile(Stream input, int bufferLength, SqlFileStream output)
        {
            byte[] fileData = new byte[bufferLength];

            using (BinaryReader br = new BinaryReader(input))
            {
                int count = 0;
                while (!br.EOF())
                {
                    long tillStreamEnd = br.BaseStream.Length - br.BaseStream.Position;
                    if (bufferLength > tillStreamEnd)
                    {
                        bufferLength = (int)tillStreamEnd;
                    }

                    fileData = br.ReadBytes(bufferLength);
                    output.Write(fileData, 0, bufferLength);
                    output.Flush();
                    count++;
                }
            }
        }
        #endregion
    }
}