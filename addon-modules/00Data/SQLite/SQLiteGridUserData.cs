/*
 * Copyright (c) Marcus Kirsch (aka Marck). All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Reflection;
using System.Data.SQLite;
using OpenSim.Data;

namespace Diva.Data.SQLite
{
    public class SQLiteGridUserData : OpenSim.Data.SQLite.SQLiteGridUserData, IGridUserData
    {
        public SQLiteGridUserData(string connectionString, string realm) 
            : base(connectionString, realm)
        {
        }

        public GridUserData[] GetOnlineUsers()
        {
            return Get("Online", true.ToString());
        }

        public long GetOnlineUserCount()
        {
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.CommandText = String.Format("select count(*) from {0} where Online='True'", m_Realm);
                    
                    lock (m_Connection)
                    {
                        cmd.Connection = m_Connection;
                        object result = cmd.ExecuteScalar();
                        return Convert.ToInt64(result);
                    }
                }
            }
            catch (System.Data.SQLite.SQLiteException)
            {
                // Tabelle existiert noch nicht während Wifi-Initialisierung
                return 0;
            }
        }

        public long GetActiveUserCount(int period)
        {
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.CommandText = String.Format("select count(*) from {0} where Online = 'True' OR CAST(julianday('now')-julianday(datetime(Logout, 'unixepoch')) AS INTEGER) <= {1}", m_Realm, period);
                    
                    lock (m_Connection)
                    {
                        cmd.Connection = m_Connection;
                        object result = cmd.ExecuteScalar();
                        return Convert.ToInt64(result);
                    }
                }
            }
            catch (System.Data.SQLite.SQLiteException)
            {
                // Tabelle existiert noch nicht während Wifi-Initialisierung
                return 0;
            }
        }

        public GridUserData[] GetUsers(string pattern)
        {
            using (SQLiteCommand cmd = new SQLiteCommand())
            {
                cmd.CommandText = String.Format("select * from {0} where UserID like ?pattern", m_Realm);
                cmd.Parameters.AddWithValue("?pattern", pattern);
                return DoQuery(cmd);
            }
        }

        public void ResetTOS()
        {
            using (SQLiteCommand cmd = new SQLiteCommand())
            {
                cmd.CommandText = String.Format("update {0} set TOS=?tos", m_Realm);
                cmd.Parameters.AddWithValue("?tos", "");
                DoQuery(cmd);
            }
        }

        public void ResetOnline()
        {
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.CommandText = String.Format("update {0} set Online='False'", m_Realm);
                    DoQuery(cmd);
                }
            }
            catch (System.Data.SQLite.SQLiteException)
            {
                // Tabelle existiert noch nicht - ignoriere Fehler bei Wifi-Start
                return;
            }
        }
    }
}
