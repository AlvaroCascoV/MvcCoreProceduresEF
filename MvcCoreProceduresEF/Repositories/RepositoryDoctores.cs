using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using MvcCoreProceduresEF.Data;
using MvcCoreProceduresEF.Models;
using System.Data;
using System.Data.Common;
using System.Numerics;
using System.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MvcCoreProceduresEF.Repositories
{
    #region STORED PROCEDURES
        //select* from DOCTOR

        //create procedure SP_ALL_ESPECIALIDADES
        //as
        //select distinct(ESPECIALIDAD) from DOCTOR
        //go
        //EXEC SP_ALL_ESPECIALIDADES

        //create procedure SP_UPDATE_DOCTOR_EF
        //(@especialidad nvarchar(50), @incremento int)
        //as
        //update DOCTOR set SALARIO += @incremento where ESPECIALIDAD=@especialidad
        //go

        //create procedure SP_DOCTORES_ESPECIALIDAD
        //(@especialidad nvarchar(50))
        //as
        //select* from DOCTOR where ESPECIALIDAD=@especialidad
        //go
    #endregion
    public class RepositoryDoctores
    {
        private EnfermosContext context;
        public RepositoryDoctores(EnfermosContext context)
        {
            this.context = context;
        }

        public async Task<List<string>> GetEspecialidadesAsync()
        {
            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                string sql = "SP_ALL_ESPECIALIDADES";
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;

                await com.Connection.OpenAsync();
                DbDataReader reader = await com.ExecuteReaderAsync();
                List<string> especialidades = new List<string>();
                while (await reader.ReadAsync())
                {
                    string especialidad = reader["ESPECIALIDAD"].ToString();
                    especialidades.Add(especialidad);
                }
                await reader.CloseAsync();
                await com.Connection.CloseAsync();
                return especialidades;
            }
        }
        public async Task<List<Doctor>> GetDoctoresEspecialidadAsync(string especialidad)
        {

            string sql = "SP_DOCTORES_ESPECIALIDAD @especialidad";
            SqlParameter pamEsp = new SqlParameter("@especialidad", especialidad);
            var consulta = this.context.Doctores.FromSqlRaw(sql, pamEsp);
            List<Doctor> doctores = await consulta.ToListAsync();
            return doctores;
        }
        public async Task UpdateSalarioAsync(string especialidad, int incremento)
        {
            string sql = "SP_UPDATE_DOCTOR_EF @especialidad, @incremento";
            SqlParameter pamEsp = new SqlParameter("@especialidad", especialidad);
            SqlParameter pamInc = new SqlParameter("@incremento", incremento);

            await this.context.Database.ExecuteSqlRawAsync(sql, pamEsp, pamInc);
        }
        
        public async Task UpdateSalarioLinqAsync(string especialidad, int incremento)
        {
            //DEBEMOS RECUPERAR LOS DATOS A MODIFICAR/ELIMINAR DESDE EL CONTEXT
            var consulta = from datos in this.context.Doctores 
                           where datos.Especialidad == especialidad 
                           select datos;
            List<Doctor> doctores = await consulta.ToListAsync();
            //esto funciona pq viene del context pero en un futuro o proyecto mas grande, puede romper
            //List <Doctor> doctores = await this.GetDoctoresEspecialidadAsync(especialidad);
            if (doctores != null)
            {
                foreach (var doctor in doctores)
                {
                    doctor.Salario += incremento;
                    await this.context.SaveChangesAsync();
                }
            }
        }
    }
}
