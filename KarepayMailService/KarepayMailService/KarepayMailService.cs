using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data.SqlClient;
using System.Configuration;
using karepay.MailService.bll;
using karepay.MailService.diagnostics;

namespace KarepayMailService
{
    public partial class KarepayMailService : ServiceBase
    {
        MailingProcess mailingProcess = null;
        private int activationTimerInterval = 300000;
        private int notificationTimerInterval = 300000;
        private Timer activationMailIntegrationTimer = null;
        private Timer notificationMailIntegrationTimer = null;

        public KarepayMailService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            initializeTimer();
        }
        
        public void initializeTimer()
        {
            WriteEventLog.WriteInformation("Mail Servisi Başladı");

            mailingProcess = new MailingProcess();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["KAREPAYDB"].ConnectionString))
            {

                string query = "select ActivationMailTimeInterval,NotificationMailTimeInterval from Common.SystemParameter";
                SqlCommand command;
                command = new SqlCommand(query, conn);
                try
                {
                    command.Connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        activationTimerInterval = Convert.ToInt32(reader["ActivationMailTimeInterval"]) * 60000;
                        notificationTimerInterval = Convert.ToInt32(reader["NotificationMailTimeInterval"]) * 60000;
                    }

                    command.Connection.Close();
                }
                catch (Exception ex)
                {
                    WriteEventLog.WriteError(ex,"Sistem Parametreleri Okunamadı!");
                }
            }
            //ProcessMailingService(null);
            activationMailIntegrationTimer = new Timer(ActivationMailingService, null, activationTimerInterval, Timeout.Infinite);
            notificationMailIntegrationTimer = new Timer(NotificationMailingService, null, notificationTimerInterval, Timeout.Infinite);
        }

        private void ActivationMailingService(object state)
        {
            mailingProcess.ActivationMailProcess();
            if (activationMailIntegrationTimer != null)
                activationMailIntegrationTimer.Change(activationTimerInterval, Timeout.Infinite);
        }

        private void NotificationMailingService(object state)
        {
            mailingProcess.NotificationMailProcess();
            if (notificationMailIntegrationTimer != null)
                notificationMailIntegrationTimer.Change(notificationTimerInterval, Timeout.Infinite);
        }

        protected override void OnStop()
        {

        }
    }
}
