using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using karepay.MailService.common.Enum;
using karepay.MailService.common.Mail;
using karepay.MailService.diagnostics;

namespace karepay.MailService.dm
{
    public class SendMail
    {

        public void SendMails(MailType mailType)
        { 
            List<Mail> mailList = new List<Mail>();


            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["KAREPAYDB"].ConnectionString))
            {

                string query = "select * from NotificationMng.Mail where Status in (@STT, @STA) and MailTypeID = @TYP";
                SqlCommand command;
                command = new SqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@STT", MailStatus.New);
                command.Parameters.AddWithValue("@STA", MailStatus.Error);
                command.Parameters.AddWithValue("@TYP", mailType);

                try
                {
                    command.Connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {

                        mailList.Add(new Mail {
                            MailID = Convert.ToInt32(reader["MailID"].ToString()),
                            From = reader["FromEmail"].ToString(),
                            To = reader["ToEmail"].ToString(),
                            Subject = reader["Subject"].ToString(),
                            Body = reader["Body"].ToString(),
                            MailType = (MailType) Convert.ToInt32(reader["MailTypeID"])
                        });
                    }
                    command.Connection.Close();
                }
                catch (Exception ex)
                {
                    WriteEventLog.WriteError(ex, "Gönderilecek Mailler Alınamadı!");
                }
            }

            foreach (Mail m in mailList){
                try
                {
                    MailParameters mailParameters = new MailParameters();
                    PrepareMailDM prepareMailDM = new PrepareMailDM();
                    mailParameters = prepareMailDM.PrepareMailParameters(m.MailType);
                    
                    string query = null;
                    SqlCommand command = null;
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient(mailParameters.SmtpAddress);

                    mail.From = new MailAddress(m.From, mailParameters.DisplayName);
                    mail.To.Add(m.To);
                    mail.Subject = m.Subject;
                    mail.IsBodyHtml = true;
                    mail.Body = m.Body;
                    

                    SmtpServer.Port = mailParameters.SmtpPort;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(mailParameters.Username, mailParameters.Password);
                    SmtpServer.EnableSsl = mailParameters.EnableSSL;

                    SmtpServer.Send(mail);

                    WriteEventLog.WriteInformation(m.To +" Adresine Mail Gönderildi!");

                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["KAREPAYDB"].ConnectionString))
                    {
                        query = "update NotificationMng.Mail set Status = @STA, SentDate = @SDATE where MailID = @ID";
                        command = new SqlCommand(query, conn);
                        command.Connection.Open();
                        command.Parameters.AddWithValue("@STA", MailStatus.Sent);
                        command.Parameters.AddWithValue("@SDATE", DateTime.Now);
                        command.Parameters.AddWithValue("@ID", m.MailID);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    string query = null;
                    SqlCommand command = null;

                    WriteEventLog.WriteError(ex, "Mail gönderiminde hata oluştu!");

                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["KAREPAYDB"].ConnectionString))
                    {
                        query = "update NotificationMng.Mail set Status = @STA, ErrorText = @ERR where MailID = @ID";
                        command = new SqlCommand(query, conn);
                        command.Connection.Open();
                        command.Parameters.AddWithValue("@STA", MailStatus.Error);
                        command.Parameters.AddWithValue("@ERR", ex.Message);
                        command.Parameters.AddWithValue("@ID", m.MailID);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

    }
}
