using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using MvcCoreProceduresEF.Data;
using MvcCoreProceduresEF.Models;
using System.Data;
using System.Diagnostics.Metrics;

#region STORED PROCEDURES
//create view V_EMPLEADOS_DEPARTAMENTOS
//as
//	select Cast(
//	isnull(ROW_NUMBER() over (order by EMP.APELLIDO) ,0)
//	as int) 	
//	as ID

//	, EMP.APELLIDO, EMP.OFICIO, EMP.SALARIO,
//    DEPT.DNOMBRE as DEPARTAMENTO, DEPT.LOC AS LOCALIDAD 
//	FROM EMP
//	inner join DEPT
//	on EMP.DEPT_NO = DEPT.DEPT_NO
//go


//CREATE procedure SP_TRABAJADORES_OFICIO
//(@oficio nvarchar(50),
//@personas int out, @media int out, @suma int out)
//as
//	select * from V_TRABAJADORES where OFICIO = @oficio
//	select @personas = count(IDTRABAJADOR), @media = avg(SALARIO), @suma = sum(SALARIO) from V_TRABAJADORES where OFICIO = @oficio
//go

//create view V_TRABAJADORES
//as
//	select EMP_NO as IDTRABAJADOR
//	, APELLIDO, OFICIO, SALARIO from EMP
//	union
//	select DOCTOR_NO, APELLIDO, ESPECIALIDAD, SALARIO from DOCTOR
//	union
//	select EMPLEADO_NO, APELLIDO, FUNCION, SALARIO from PLANTILLA

//go
#endregion

namespace MvcCoreProceduresEF.Repositories
{
    public class RepositoryEmpleados
    {
        private HospitalContext context;

        public RepositoryEmpleados(HospitalContext context)
        {
            this.context = context;
        }

        public async Task<List<VistaEmpleado>> GetVistaEmpleadosAsync()
        {
            var consulta = from datos in this.context.VistaEmpleados select datos;
            return await consulta.ToListAsync();
        }

        public async Task<TrabajadoresModel> GetTrabajadoresModelAsync()
        {
            //PRIMERO CON LINQ
            var consulta = from datos in this.context.Trabajadores select datos;
            TrabajadoresModel model = new TrabajadoresModel();
            model.Trabajadores = await consulta.ToListAsync();
            model.Personas = await consulta.CountAsync();
            model.SumaSalarial = await consulta.SumAsync(z => z.Salario);
            model.MediaSalarial = (int) await consulta.AverageAsync(x => x.Salario);
            return model;
        }

        public async Task<List<string>> GetOficiosAsync()
        {
            var consulta = (from datos in this.context.Trabajadores select datos.Oficio).Distinct();
            return await consulta.ToListAsync();
        }
        public async Task<TrabajadoresModel> GetTrabajadoresModelOficioAsync(string oficio)
        {
            //YA QUE TENEMOS MODEL, VAMOS A LLAMARLO CON EF
            //LA UNICA DIFERENCIA CUANDO TENEMOS PARAMETROS DE SALIDA
            //ES INDICAR LA PALABRA OUT EN LA DECLARACION DE LAS VARIABLES
            string sql = "SP_TRABAJADORES_OFICIO @oficio, @personas out, @media out, @suma out";
            SqlParameter pamOficio = new SqlParameter("@oficio", oficio);
            SqlParameter pamPersonas = new SqlParameter("@personas", -1);
            pamPersonas.Direction = ParameterDirection.Output;
            SqlParameter pamMedia = new SqlParameter("@media", -1);
            pamMedia.Direction = ParameterDirection.Output;
            SqlParameter pamSuma = new SqlParameter("@suma", -1);
            pamSuma.Direction = ParameterDirection.Output;
            //EJECUTAMOS LA CONSULTA CON EL MODEL FromSqlRaw
            var consulta = this.context.Trabajadores.FromSqlRaw(sql, pamOficio, pamPersonas, pamMedia, pamSuma);
            TrabajadoresModel model = new TrabajadoresModel();
            //Hay que poner primero los trabajadores pq sino no recorre los datos
            model.Trabajadores = await consulta.ToListAsync();
            model.Personas = int.Parse(pamPersonas.Value.ToString());
            model.MediaSalarial = int.Parse(pamMedia.Value.ToString());
            model.SumaSalarial = int.Parse(pamSuma.Value.ToString());
            return model;
        }
    }
}
