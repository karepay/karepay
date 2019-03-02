using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using karepay.MailService.common.Enum;
using karepay.MailService.common.Mail;
using karepay.MailService.common.OperationResults;
using karepay.MailService.diagnostics;
using karepay.MailService.dm;
using karepay.MailService.Interfaces;

namespace karepay.MailService.Integration
{
    public class NotificationMail : IMailService
    {

        Mail mail = null;
        PrepareMailDM prepareMail = null;


        public void NotificationMailProcess()
        {
            mail = new Mail();
            prepareMail = new PrepareMailDM();

            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["KAREPAYDB"].ConnectionString))
            {
                string query = "select NTF.NotificationID,NTF.Text,USR.Email from  NotificationMng.Notifications NTF " +
                                    "inner join ProfileMng.UserAccount USR on USR.UserAccountID = NTF.UserAccountID " +
                                    "where NTF.Status = @STA";
                SqlCommand command;
                command = new SqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@STA", MailStatus.New);

                try
                {
                    command.Connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    OperationResult operationResult = null;

                    while (reader.Read())
                    {
                        string notificationText = reader["Text"].ToString();
                        int notificationID = Convert.ToInt32(reader["NotificationID"]);
                        operationResult = new OperationResult();
                        mail = PrepareMail(MailType.NotificationMail);
                        mail.To = reader["Email"].ToString();

                        operationResult = SaveMail(mail, notificationText);

                        if (operationResult.result)
                        {
                            WriteEventLog.WriteInformation("Yeni Mail Oluşturuldu!");

                            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["KAREPAYDB"].ConnectionString))
                            {
                                query = "update NotificationMng.Notifications set Status = @STA where NotificationID = @ID";
                                command = new SqlCommand(query, conn);
                                command.Connection.Open();
                                command.Parameters.AddWithValue("@ID", notificationID);
                                command.Parameters.AddWithValue("@STA", MailStatus.Sending);
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    WriteEventLog.WriteError(ex, "Bilgilendirme Tablosu Güncellerken Hata!");
                                }
                            }
                        }
                        else
                        {
                            WriteEventLog.WriteError(operationResult.exception, "Mail Oluşturulurken Hata! \n");
                        }
                    }
                    command.Connection.Close();
                }
                catch (Exception ex)
                {
                    WriteEventLog.WriteError(ex, "Mail Parametreleri Alınamadı!");
                }
            }

            SendMail(MailType.NotificationMail);
        }

        public Mail PrepareMail(MailType mailType)
        {
            return prepareMail.PrepareMail(MailType.NotificationMail);
        }

        public OperationResult SaveMail(Mail mail, string text)
        {
            return prepareMail.SaveMail(mail, text);
        }

        public void SendMail(MailType mailType)
        {
            SendMail sendMail = new SendMail();
            sendMail.SendMails(mailType);
        }
    }
}
