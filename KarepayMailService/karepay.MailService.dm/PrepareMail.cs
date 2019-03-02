using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using karepay.MailService.common.Enum;
using karepay.MailService.common.Mail;
using karepay.MailService.common.OperationResults;
using System.Data;
using System.Diagnostics;
using karepay.MailService.diagnostics;

namespace karepay.MailService.dm
{
    public class PrepareMailDM
    {
        Mail mail;

        public Mail PrepareMail(MailType mailType)
        {
            mail = new Mail();

            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["KAREPAYDB"].ConnectionString))
            {

                string query = "select * from NotificationMng.MailFormat MF " +
                                    "inner join NotificationMng.MailAdress MA on MF.MailTypeID = MF.MailTypeID and MA.Active = @ACT and MF.Locked = @LCK " +
                                    "where MF.MailTypeID = @TYP";
                SqlCommand command;
                command = new SqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@TYP", mailType);
                command.Parameters.AddWithValue("@ACT", 1);
                command.Parameters.AddWithValue("@LCK", 0);

                try
                {
                    command.Connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        mail.DisplayName = reader["DisplayName"].ToString();
                        mail.Subject = reader["Subject"].ToString();
                        mail.Body = reader["Body"].ToString();
                        mail.MailType = mailType;
                        //mail.To = reader["T"].ToString();
                        mail.MailParameters = new MailParameters();
                        mail.MailParameters.SmtpAddress = reader["SmtpAddress"].ToString();
                        mail.MailParameters.SmtpPort = Convert.ToInt32(reader["SmtpPort"]);
                        mail.MailParameters.EnableSSL = Convert.ToBoolean(reader["SmtpPort"]);
                        mail.MailParameters.From = reader["FromEmail"].ToString();
                        mail.MailParameters.Username = reader["UserName"].ToString();
                        mail.MailParameters.Password = reader["Password"].ToString();

                    }
                    command.Connection.Close();
                }
                catch (Exception ex)
                {
                    WriteEventLog.WriteError(ex, "Mail Parametreleri Alınamadı!");
                }
            }

            return mail;

        }


        public OperationResult SaveMail(Mail mail,string activationLink)
        {
            OperationResult operationResult = new OperationResult();
            operationResult.result = true;
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["KAREPAYDB"].ConnectionString))
            {
                SqlCommand command = null;
                try
                {
                    string query = "insert into NotificationMng.Mail (FromEmail,ToEmail,Subject,Body,RequestDate,Status,SentDate,ErrorText,MailTypeID)  values ( @FROM, @TO, @SUB, @BODY, @REQDATE, @STATUS, @SENTDATE, @ERR, @TYP)";
                    
                    command = new SqlCommand(query, sqlConnection);
                    command.Connection.Open();
                    command.Parameters.Add("@FROM", SqlDbType.NVarChar, 100).Value = mail.MailParameters.From;
                    command.Parameters.Add("@TO", SqlDbType.NVarChar, 100).Value = mail.To;
                    command.Parameters.Add("@SUB", SqlDbType.NVarChar, Int32.MaxValue).Value = mail.Subject;
                    command.Parameters.Add("@BODY", SqlDbType.NVarChar, Int32.MaxValue).Value = String.Format(mail.Body, activationLink);
                    command.Parameters.Add("@REQDATE", SqlDbType.DateTime).Value = DateTime.Now;
                    command.Parameters.Add("@STATUS", SqlDbType.Int).Value = MailStatus.New;
                    command.Parameters.Add("@SENTDATE", SqlDbType.DateTime).Value = DBNull.Value;
                    command.Parameters.Add("@ERR", SqlDbType.NVarChar, Int32.MaxValue).Value = DBNull.Value;
                    command.Parameters.Add("@TYP", SqlDbType.TinyInt).Value = mail.MailType;

                    command.ExecuteNonQuery();


                }
                catch (Exception ex)
                {
                    operationResult.result = false;
                    operationResult.exception = new Exception();
                    operationResult.exception = ex;
                }


                command.Connection.Close();

            }

            return operationResult;
        }

        public MailParameters PrepareMailParameters(MailType mailType)
        {
            MailParameters mailParameters = new MailParameters();

            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["KAREPAYDB"].ConnectionString))
            {

                string query = "select * from NotificationMng.MailAdress where MailTypeID = @TYP";
                SqlCommand command;
                command = new SqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@TYP", mailType);

                try
                {
                    command.Connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        mailParameters.SmtpAddress = reader["SmtpAddress"].ToString();
                        mailParameters.SmtpPort = Convert.ToInt32(reader["SmtpPort"]);
                        mailParameters.EnableSSL = Convert.ToBoolean(reader["EnableSll"]);
                        mailParameters.From = reader["FromEmail"].ToString();
                        mailParameters.Username = reader["UserName"].ToString();
                        mailParameters.Password = reader["Password"].ToString();
                        mailParameters.DisplayName = reader["DisplayName"].ToString();

                    }
                    command.Connection.Close();
                }
                catch (Exception ex)
                {
                    WriteEventLog.WriteError(ex, "Mail Parametreleri Alınamadı!");
                }
            }

            return mailParameters;
        }
    }
}
