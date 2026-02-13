using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using MvcCoreProceduresEF.Data;
using MvcCoreProceduresEF.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Metrics;

namespace MvcCoreProceduresEF.Repositories
{

    #region STORED PROCEDURES

//    select* from enfermo

//create procedure SP_ALL_ENFERMOS
//as
//select* from ENFERMO
//go

//create procedure SP_FIND_ENFERMO
//(@inscripcion nvarchar(50))
//as
//select* from ENFERMO where INSCRIPCION=@inscripcion
//go

//create procedure SP_DELETE_ENFERMO
//(@inscripcion nvarchar(50))
//as
//delete from ENFERMO where INSCRIPCION=@inscripcion
//go

//create procedure SP_INSERT_ENFERMO
//(@apellido nvarchar(50), @direccion nvarchar(50), @fechanac datetime, @sexo nvarchar(50), @nss nvarchar(50))
//as
//	declare @inscripcion nvarchar(50);
//    select @inscripcion = (select cast(MAX(INSCRIPCION) as int) from ENFERMO ) + 1;
//	insert into ENFERMO values(@inscripcion, @apellido, @direccion, @fechanac, @sexo, @nss)
//go

//exec SP_INSERT_ENFERMO casco, casa,1234,n,1543

    #endregion

    public class RepositoryEnfermos
    {
        private HospitalContext context;
        public RepositoryEnfermos(HospitalContext context)
        {
            this.context = context;
        }
        public async Task<List<Enfermo>> GetEnfermosAsync()
        {
            //NECESITAMOS UN COMMAND, VAMOS A UTILIZAR UN using PARA TODO
            //EL COMMAND, EN SU CREACION, NECESITA UNA CADENA DE CONEXION(OBJETO)
            //EL OBJETO CONNECTION NOS LO OFRECE EF
            //las conexiones se crean a partir del context
            using(DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                string sql = "SP_ALL_ENFERMOS";
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;
                //ABRIMOS LA CONEXION
                await com.Connection.OpenAsync();
                //EJECUTAMOS NUESTRO READER
                DbDataReader reader = await com.ExecuteReaderAsync();
                //DEBEMOS MAPEAR LOS DATOS MANUALMENTE
                List<Enfermo> enfermos = new List<Enfermo>();
                while(await reader.ReadAsync())
                {
                    Enfermo enfermo = new Enfermo 
                    {
                        Inscripcion = reader["INSCRIPCION"].ToString(),
                        Apellido = reader["APELLIDO"].ToString(),
                        Direccion = reader["DIRECCION"].ToString(),
                        FechaNacimiento = DateTime.Parse(reader["FECHA_NAC"].ToString()),
                        Genero = reader["S"].ToString(),
                        Nss = reader["NSS"].ToString()
                    };
                    enfermos.Add(enfermo);
                }
                await reader.CloseAsync();
                await com.Connection.CloseAsync();
                return enfermos;
            }
        }

        public async Task<Enfermo> FindEnfermoAsync(string inscripcion)
        {
            //PARA LLAMAR A UN PROCEDIMIENTO QUE CONTIENE PARAMETROS
            //LA LLAMADA SE REALIZA MEDIANTE EL NOMBRE DEL PROCEDURE
            //Y CADA PARAMETRO A CONTINUACION EN LA DECLARACION
            //DEL SQL: SP_PROCEDURE @PAM1, @PAM2
            string sql = "SP_FIND_ENFERMO @inscripcion";
            SqlParameter pamIns = new SqlParameter("@inscripcion", inscripcion);
            //SI LOS DATOS QUE DEVUELVE EL PROCEDURE ESTAN MAPEADOS
            //CON UN MODEL, PODEMOS UTILIZAR EL METODO
            //FromSqlRaw PARA RECUPERAR DIRECTAMENTE EL MODEL/S.
            //NO PODEMOS CONSULTAR Y EXTRAER A LA VEX CON LINQ, SE DEBE
            //REALIZAR SIEMPRE EN DOS PASOS
            //hay que hacerlo async
            var consulta = this.context.Enfermos.FromSqlRaw(sql, pamIns);
            Enfermo enfermo = await consulta.ToAsyncEnumerable().FirstOrDefaultAsync();
            //otra forma
            //var consulta = this.context.Enfermos.FromSqlRaw(sql, pamIns).ToListAsync();
            //Enfermo enfermo = await consulta.FirstOrDefault();

            //DEBEMOS UTILIZAR AsEnumerable() PARA EXTRAER LOS DATOS
            //Enfermo enfermo = consulta.AsEnumerable().FirstOrDefault();
            return enfermo;
        }

        public async Task DeleteEnfermoAsync(string inscripcion)
        {
            string sql = "SP_DELETE_ENFERMO";
            SqlParameter pamIns = new SqlParameter("@inscripcion", inscripcion);
            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;
                com.Parameters.Add(pamIns);
                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
                com.Parameters.Clear();
            }
        }
        public async Task DeleteEnfermoRawAsync(string inscripcion)
        {
            string sql = "SP_DELETE_ENFERMO @inscripcion";
            SqlParameter pamIns = new SqlParameter("@inscripcion", inscripcion);
            await this.context.Database.ExecuteSqlRawAsync(sql, pamIns);
        }

        public async Task InsertEnfermoAsync(string apellido, string direccion, DateTime fechanac, string sexo, string nss)
        {
            string sql = "SP_INSERT_ENFERMO";
            SqlParameter pamApe = new SqlParameter("@apellido", apellido);
            SqlParameter pamDir = new SqlParameter("@direccion", direccion);
            SqlParameter pamFN = new SqlParameter("@fechanac", fechanac);
            SqlParameter pamSex = new SqlParameter("@sexo", sexo);
            SqlParameter pamNss = new SqlParameter("@nss", nss);

            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;
                
                com.Parameters.Add(pamApe);
                com.Parameters.Add(pamDir);
                com.Parameters.Add(pamFN);
                com.Parameters.Add(pamSex);
                com.Parameters.Add(pamNss);

                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
                com.Parameters.Clear();
            }
        }
        public async Task InsertEnfermoRawAsync(string apellido, string direccion, DateTime fechanac, string sexo, string nss)
        {
            string sql = "SP_INSERT_ENFERMO @apellido, @direccion, @fechanac, @sexo, @nss";
            SqlParameter pamApe = new SqlParameter("@apellido", apellido);
            SqlParameter pamDir = new SqlParameter("@direccion", direccion);
            SqlParameter pamFN = new SqlParameter("@fechanac", fechanac);
            SqlParameter pamSex = new SqlParameter("@sexo", sexo);
            SqlParameter pamNss = new SqlParameter("@nss", nss);

            await this.context.Database.ExecuteSqlRawAsync(sql, pamApe, pamDir, pamFN, pamSex, pamNss);
        }
    }
}
