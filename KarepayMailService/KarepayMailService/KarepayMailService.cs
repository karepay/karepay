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
        private int timerInterval = 300000;
        private Timer integrationTimer = null;

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

                string query = "select MailServiceTimeInterval from Common.SystemParameter";
                SqlCommand command;
                command = new SqlCommand(query, conn);
                try
                {
                    command.Connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                        timerInterval = Convert.ToInt32(reader["MailServiceTimeInterval"]) * 60000;
                    

                    command.Connection.Close();
                }
                catch (Exception ex)
                {
                    WriteEventLog.WriteError(ex,"Sistem Parametreleri Okunamadı!");
                }
            }
            //ProcessMailingService(null);
            integrationTimer = new Timer(ProcessMailingService,null, timerInterval, Timeout.Infinite);
        }

        private void ProcessMailingService(object state)
        {
            mailingProcess.MailProcess();
            if (integrationTimer != null)
                integrationTimer.Change(timerInterval, Timeout.Infinite);
        }

        protected override void OnStop()
        {

        }
    }
}
