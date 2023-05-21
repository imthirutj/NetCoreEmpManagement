using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Data;
using EmployeeManagement.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Cryptography;
using EmployeeManagement.Service;
using Microsoft.AspNetCore.Cors;

namespace EmployeeManagement.Controllers
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeDbContext _dbContext;
        private readonly IConfiguration _configuration;


        public EmployeeController(EmployeeDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration; 
        }

        // GET: api/employees
        [HttpGet]
        public ActionResult<IEnumerable<Employee>> GetAllEmployees()
        {
            return Ok(_dbContext.Employees);
        }

        // GET: api/employees/{id}
        [HttpGet("{id}")]
        public ActionResult<Employee> GetEmployeeById(int id)
        {
            var employee = _dbContext.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }

            return Ok(employee);
        }



        [HttpPost]
        public async Task<ActionResult<Employee>> CreateEmployee(Employee employee)
        {
            try
            {
                _dbContext.Employees.Add(employee);
                await _dbContext.SaveChangesAsync();

                // Return the generated ID
                return employee;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while creating the employee." + ex);
            }
        }



        [HttpGet("login")]
        public ActionResult<Employee> Login(string username, string password)
        {
            try
            {
                var employee = _dbContext.Employees.FirstOrDefault(e => e.username == username && e.password == password);

                if (employee != null)
                {
                    // Employee found
                    return Ok(employee);
                }
                else
                {
                    // Employee not found
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while checking the employee login." + ex);
            }
        }

        [HttpGet("loginReturnTok")]
        public ActionResult LoginReturntok(string username, string password)
        {
            try
            {
                var employee = _dbContext.Employees.FirstOrDefault(e => e.username == username && e.password == password);

                if (employee != null)
                {
                    // Employee found, generate JWT and refresh token
                    var tokenHandler = new JwtSecurityTokenHandler();
                 
                    //var keyBytes = new byte[32]; // 32 bytes = 256 bits
                    //using (var rng = new RNGCryptoServiceProvider())
                    //{
                    //    rng.GetBytes(keyBytes);
                    //}
                    //var key1 = new SymmetricSecurityKey(keyBytes);

                    var keyString = _configuration.GetSection("JwtSecretKey").Value;
                    var keyBytes = Encoding.UTF8.GetBytes(keyString);

                    var key = new SymmetricSecurityKey(keyBytes);

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
        new Claim(ClaimTypes.Name, employee.username),
                            // Add additional claims if needed
                        }),
                        Expires = DateTime.UtcNow.AddDays(7), // Set the expiration time for the token
                        SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
                    };


                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var jwt = tokenHandler.WriteToken(token);

                    // Generate refresh token (can be stored in a database or other storage)
                    var refreshToken = new utilities().GenerateRefreshToken();

                    // Store the refresh token for the user (e.g., associate it with the user in the database)

                    // Return the JWT and refresh token
                    return Ok(new { Token = jwt, RefreshToken = refreshToken });
                }
                else
                {
                    // Employee not found
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while checking the employee login. " + ex.Message);
            }
        }




    }
}
