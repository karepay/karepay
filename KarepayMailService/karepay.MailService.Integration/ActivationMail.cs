using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using karepay.MailService.common.Enum;
using karepay.MailService.common.Mail;
using karepay.MailService.common.OperationResults;
using karepay.MailService.Interfaces;
using karepay.MailService.dm;
using karepay.MailService.diagnostics;
using System.Data.SqlClient;
using System.Configuration;

namespace karepay.MailService.Integration
{
    public class ActivationMail : IMailService
    {

        Mail mail = null;
        PrepareMailDM prepareMail = null;

        public void ActivationMailProcess()
        {
            mail = new Mail();
            prepareMail = new PrepareMailDM();

            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["KAREPAYDB"].ConnectionString))
            {
                string query = "select UAC.ActivationKey,USR.Email from ProfileMng.UserAccountActivation UAC " +
                                    "inner join ProfileMng.UserAccount USR on USR.UserAccountID = UAC.UserAccountID " +
                                    "where UAC.Status = @STA";
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
                        string actKey = reader["ActivationKey"].ToString();
                        operationResult = new OperationResult();
                        mail = PrepareMail(MailType.AuthenticationMail);
                        mail.To = reader["Email"].ToString();

                        operationResult = SaveMail(mail, actKey);

                        if (operationResult.result)
                        {
                            WriteEventLog.WriteInformation("Yeni Mail Oluşturuldu!");

                            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["KAREPAYDB"].ConnectionString))
                            {
                                query = "update ProfileMng.UserAccountActivation set Status = @STA where ActivationKey = @ACTKEY";
                                command = new SqlCommand(query, conn);
                                command.Connection.Open();
                                command.Parameters.AddWithValue("@ACTKEY", actKey);
                                command.Parameters.AddWithValue("@STA", MailStatus.Sending);
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    WriteEventLog.WriteError(ex, "Aktivasyon Tablosu Güncellerken Hata! \n" + "Aktivasyon UUID: " + actKey);
                                }
                            }
                        }
                        else
                        {
                            WriteEventLog.WriteError(operationResult.exception, "Mail Oluşturulurken Hata! \n" + "Aktivasyon UUID: " + actKey);
                        }
                    }
                    command.Connection.Close();
                }
                catch (Exception ex)
                {
                    WriteEventLog.WriteError(ex, "Mail Parametreleri Alınamadı!");
                }
            }

            SendMail(MailType.AuthenticationMail);
        }

        public Mail PrepareMail(MailType mailType)
        {
            return prepareMail.PrepareMail(MailType.AuthenticationMail);
        }

        public OperationResult SaveMail(Mail mail, string activationLink)
        {
            return prepareMail.SaveMail(mail, activationLink);
        }

        public void SendMail(MailType mailType)
        {
            SendMail sendMail = new SendMail();
            sendMail.SendMails(mailType);
        }
    }
}
