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
        public async Task<IActionResult> Get(
            [FromQuery] int? neighborhoodId)
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
                    if (neighborhoodId != null)
                    {
                        cmd.CommandText += " WHERE w.NeighborhoodId = @neighborhoodID";
                        cmd.Parameters.Add(new SqlParameter("neighborhoodID", neighborhoodId));
                    }

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

        //----------GET by Id----------
        [HttpGet("{id}", Name = "GetWalker")]
        public async Task<IActionResult> Get(
            [FromRoute] int id,
            [FromQuery] string include)
        {
            if (include != "walks")
            {
                var walker = GetWalker(id);
                return Ok(walker);
            }
            else
            {
                var walker = GetWalkerWithWalks(id);
                return Ok(walker);
            }
        }

        private Walker GetWalker(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT w.Id, w.Name, w.NeighborhoodId, n.Id, n.Name AS NeighborhoodName
                        FROM Walker w
                        LEFT JOIN Neighborhood n
                        ON w.NeighborhoodId = n.Id
                        WHERE w.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Walker walker = null;

                    if (reader.Read())
                    {
                        walker = new Walker()
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
                    }
                    reader.Close();

                    return walker;
                }
            }
        }

        private Walker GetWalkerWithWalks(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT w.Id, w.Name, w.NeighborhoodId, n.Id, n.Name AS NeighborhoodName, wa.Id as WalksId, wa.Date, wa.Duration, wa.DogId
                        FROM Walker w
                        LEFT JOIN Neighborhood n
                        ON w.NeighborhoodId = n.Id
                        LEFT JOIN Walks wa 
                        ON wa.WalkerId = w.Id
                        WHERE w.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Walker walker = null;

                    while (reader.Read())
                    {
                        
                        if (walker == null)
                        {
                            walker = new Walker
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Neighborhood = new Neighborhood()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                    Name = reader.GetString(reader.GetOrdinal("NeighborhoodName"))
                                },
                                walks = new List<Walks>()
                            };
                        }
                        walker.walks.Add(new Walks()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("WalksId")),
                            Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                            Duration = reader.GetInt32(reader.GetOrdinal("Duration")),
                            WalkerId = reader.GetInt32(reader.GetOrdinal("Id")),
                            DogId = reader.GetInt32(reader.GetOrdinal("DogId"))
                        });
                    }
                    reader.Close();
                    return walker;
                }
            }
        }


        ////----------POST----------

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Walker walker)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO Walker (Name, NeighborhoodId)
                        OUTPUT INSERTED.Id
                        VALUES (@Name,  @NeighborhoodId)";
                    cmd.Parameters.Add(new SqlParameter("@Name", walker.Name));
                    cmd.Parameters.Add(new SqlParameter("@NeighborhoodId", walker.NeighborhoodId));

                    int id = (int)cmd.ExecuteScalar();

                    walker.Id = id;
                    return CreatedAtRoute("GetWalker", new { id = id }, walker);
                }
            }
        }

        ////----------PUT----------
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Walker walker)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Walker
                                     SET Name = @name, NeighborhoodId = @neighborhoodId
                                     WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@Name", walker.Name));
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodId", walker.NeighborhoodId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!WalkerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        ////----------DELETE----------

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM Walker WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!WalkerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool WalkerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, NeighborhoodId
                        FROM Walker
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
