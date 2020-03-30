using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DogWalkerAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DogWalkerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OwnerController(IConfiguration config)
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
            [FromQuery] string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    
                    if (include == "neighborhood")
                    {
                        cmd.CommandText += @" 
                                           SELECT o.Id, o.[Name], o.Address, o.Phone, o.NeighborhoodId, n.Name as Neighborhoodname, n.Id
                                           FROM OWNER o 
                                           LEFT JOIN Neighborhood n
                                           ON o.NeighborhoodId = n.Id
                                           SELECT n.[Name] as NeighborhoodName
                                           From Neighborhood n";
                      

                        SqlDataReader reader = cmd.ExecuteReader();

                    List<Owner> allOwners = new List<Owner>();

                    while (reader.Read())
                    {
                        var owner = new Owner()
                        {

                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                            Neighborhood = new Neighborhood()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Name = reader.GetString(reader.GetOrdinal("NeighborhoodName"))

                            },

                        };

                        allOwners.Add(owner);
                    }
                    reader.Close();

                    return Ok(allOwners);
                    } else
                    {
                        cmd.CommandText = @"
                        SELECT o.Id, o.[Name], o.Address, o.Phone, o.NeighborhoodId
                        FROM OWNER o 
                        ";

                        SqlDataReader reader = cmd.ExecuteReader();

                        List<Owner> allOwners = new List<Owner>();

                        while (reader.Read())
                        {
                            var owner = new Owner()
                            {

                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                            };

                            allOwners.Add(owner);
                        }
                        reader.Close();

                        return Ok(allOwners);
                    }
                }
            }
        }

        //----------GET by Id----------
        [HttpGet("{id}", Name = "GetOwner")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT o.Id, o.[Name],  o.Phone, o.Address, o.NeighborhoodId, n.[Name] as NeighborhoodName, n.Id, d.Id as DogId, d.Name as DogName, d.Breed, d.Notes
                        FROM OWNER o
                        LEFT JOIN Neighborhood n
                        ON o.NeighborhoodId = n.Id
                        LEFT JOIN Dog d
                        ON d.OwnerId = o.Id
                        WHERE o.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Owner owner = null;

                    while (reader.Read())
                    {

                        if (owner == null)
                        {

                            owner = new Owner()
                            {

                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Neighborhood = new Neighborhood()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                    Name = reader.GetString(reader.GetOrdinal("NeighborhoodName")),

                                },

                                OwnersDogs = new List<Dog>()
                            };
                        }
                            owner.OwnersDogs.Add(new Dog()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DogId")),
                                Name = reader.GetString(reader.GetOrdinal("DogName")),
                                Breed = reader.GetString(reader.GetOrdinal("Breed")),
                                Notes = reader.GetString(reader.GetOrdinal("Notes"))

                            });
                        }
                            
                        
                        reader.Close();

                        return Ok(owner);
                    
                    }
                }
            }
        

        ////////----------POST----------

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Owner owner)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO OWNER (Name, Address, NeighborhoodId, Phone )
                        OUTPUT INSERTED.Id
                        VALUES (@Name,  @Address, @NeighborhoodId, @Phone)";
                    cmd.Parameters.Add(new SqlParameter("@Name", owner.Name));
                    cmd.Parameters.Add(new SqlParameter("@Address", owner.Address));
                    cmd.Parameters.Add(new SqlParameter("@NeighborhoodId", owner.NeighborhoodId));
                    cmd.Parameters.Add(new SqlParameter("@Phone", owner.Phone));

                    int id = (int)cmd.ExecuteScalar();

                    owner.Id = id;
                    return CreatedAtRoute("GetOwner", new { id = id }, owner);
                }
            }
        }

        ////////----------PUT----------
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Owner owner)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE OWNER
                                     SET Name = @name, NeighborhoodId = @neighborhoodId, Address = @Address, Phone = @Phone
                                     WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@Name", owner.Name));
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodId", owner.NeighborhoodId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@Address", owner.Address));
                        cmd.Parameters.Add(new SqlParameter("@Phone", owner.Phone));


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
                if (!OwnerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }




        private bool OwnerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name
                        FROM Owner
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}