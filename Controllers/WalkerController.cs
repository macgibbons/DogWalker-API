using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using DogWalkerAPI.Models;
using Microsoft.AspNetCore.Http;

namespace DogWalkerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalkerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public WalkerController(IConfiguration config)
        {
            _config = config;
        }
        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // ----------Get all----------
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT w.Id, w.Name, w.NeighborhoodId, n.Id, n.Name AS NeighborhoodName
                        FROM Walker w
                        LEFT JOIN Neighborhood n
                        ON w.NeighborhoodId = n.Id";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Walker> allWalkers = new List<Walker>();

                    while (reader.Read())
                    {
                        var walker = new Walker()
                        {
                         
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                            Neighborhood = new Neighborhood()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Name = reader.GetString(reader.GetOrdinal("NeighborhoodName"))
                            }
                        };

                        allWalkers.Add(walker);
                    }
                    reader.Close();

                    return Ok(allWalkers);
                }
            }
        }

        ////----------GET by Id----------
        //[HttpGet("{id}", Name = "GetWalker")]
        //public async Task<IActionResult> Get([FromRoute] int id)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"SELECT e.Id, e.FirstName, e.LastName, e.DepartmentId, d.Id, d.DeptName
        //                FROM Walker e
        //                LEFT JOIN Department d
        //                ON e.DepartmentId = d.Id
        //                WHERE e.Id = @id";
        //            cmd.Parameters.Add(new SqlParameter("@id", id));
        //            SqlDataReader reader = cmd.ExecuteReader();

        //            Walker walker = null;

        //            if (reader.Read())
        //            {
        //                walker = new Walker
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
        //                    LastName = reader.GetString(reader.GetOrdinal("LastName")),

        //                    DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
        //                    Department = new Department
        //                    {
        //                        Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
        //                        DeptName = reader.GetString(reader.GetOrdinal("DeptName"))
        //                    }
        //                };
        //                reader.Close();

        //                return Ok(walker);
        //            }
        //            else
        //            {
        //                return NotFound();
        //            }
        //        }
        //    }
        //}

        ////----------POST----------

        //[HttpPost]
        //public async Task<IActionResult> Post([FromBody] Walker walker)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"
        //                INSERT INTO Walker (FirstName, LastName, DepartmentId)
        //                OUTPUT INSERTED.Id
        //                VALUES (@firstName, @lastName, @departmentId)";
        //            cmd.Parameters.Add(new SqlParameter("@firstName", walker.FirstName));
        //            cmd.Parameters.Add(new SqlParameter("@lastName", walker.LastName));
        //            cmd.Parameters.Add(new SqlParameter("@DepartmentId", walker.DepartmentId));

        //            int id = (int)cmd.ExecuteScalar();

        //            walker.Id = id;
        //            return CreatedAtRoute("GetWalker", new { id = id }, walker);
        //        }
        //    }
        //}

        ////----------PUT----------
        //[HttpPut("{id}")]
        //public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Walker walker)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = Connection)
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.CommandText = @"UPDATE Walker
        //                             SET FirstName = @firstName, LastName = @lastName, DepartmentId = @departmentId
        //                             WHERE Id = @id";

        //                cmd.Parameters.Add(new SqlParameter("@firstName", walker.FirstName));
        //                cmd.Parameters.Add(new SqlParameter("@lastName", walker.LastName));
        //                cmd.Parameters.Add(new SqlParameter("@departmentId", walker.DepartmentId));
        //                cmd.Parameters.Add(new SqlParameter("@id", id));

        //                int rowsAffected = cmd.ExecuteNonQuery();
        //                if (rowsAffected > 0)
        //                {
        //                    return new StatusCodeResult(StatusCodes.Status204NoContent);
        //                }
        //                throw new Exception("No rows affected");
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        if (!WalkerExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}

        ////----------DELETE----------

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete([FromRoute] int id)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = Connection)
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.CommandText = "DELETE FROM Walker WHERE Id = @id";
        //                cmd.Parameters.Add(new SqlParameter("@id", id));

        //                int rowsAffected = cmd.ExecuteNonQuery();
        //                if (rowsAffected > 0)
        //                {
        //                    return new StatusCodeResult(StatusCodes.Status204NoContent);
        //                }
        //                throw new Exception("No rows affected");
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        if (!WalkerExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}

        //private bool WalkerExists(int id)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"
        //                SELECT Id, firstName, lastName,departmentId
        //                FROM Walker
        //                WHERE Id = @id";
        //            cmd.Parameters.Add(new SqlParameter("@id", id));

        //            SqlDataReader reader = cmd.ExecuteReader();
        //            return reader.Read();
        //        }
        //    }
        //}
    }
}
