using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProEventos.Api.Helpers;
using ProEventos.API.Extensions;
using ProEventos.Application.Contratos;
using ProEventos.Application.Dtos;
using ProEventos.Domain;

namespace ProEventos.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ITokenService _tokenService;
        private readonly IUtil _util;

        private readonly string _destino = "Perfil";

        public AccountController(IAccountService accountService,
                                 ITokenService tokenService,
                                 IUtil util)
        {
            _util = util;
            _accountService = accountService;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Retornar informações de usuário
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpGet("GetUser")]
        [ProducesResponseType(typeof(SucessRetorno<UserUpdateDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var userName = User.GetUserName();
                var user = await _accountService.GetUserByUserNameAsync(userName);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar recuperar Usuário. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Realizar cadastro de um novo usuário
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <remarks>
        /// Exemplo de requisição
        /// 
        ///      POST /api/Account/Register
        ///      {
        ///          "userName": "Nome de usuário",
        ///          "email": "Email do usuário",
        ///          "password": "Senha do usuário",
        ///          "primeiroNome": "Nome do usuário",
        ///          "ultimoNome": "Sobrenome do usuário"
        ///      }
        /// </remarks>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpPost("Register")]
        [ProducesResponseType(typeof(SucessRetorno<UserRegisterDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            try
            {
                if (await _accountService.UserExists(userDto.UserName))
                    return BadRequest("Usuário já existe");

                var user = await _accountService.CreateAccountAsync(userDto);
                if (user != null)
                    return Ok(new
                    {
                        userName = user.UserName,
                        PrimeroNome = user.PrimeiroNome,
                        token = _tokenService.CreateToken(user).Result
                    });

                return BadRequest("Usuário não criado, tente novamente mais tarde!");
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar Registrar Usuário. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Realizar login de usuário
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <remarks>
        /// Exemplo de requisição
        /// 
        ///      POST /api/Account/Login
        ///      {
        ///          "userName": "Nome de usuário",
        ///          "password": "Senha do usuário"
        ///      }
        /// </remarks>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpPost("Login")]
        [ProducesResponseType(typeof(SucessRetorno<UserRegisterDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto userLogin)
        {
            try
            {
                var user = await _accountService.GetUserByUserNameAsync(userLogin.Username);
                if (user == null) return Unauthorized("Usuário ou Senha está errado");

                var result = await _accountService.CheckUserPasswordAsync(user, userLogin.Password);
                if (!result.Succeeded) return Unauthorized();

                return Ok(new
                {
                    userName = user.UserName,
                    PrimeroNome = user.PrimeiroNome,
                    token = _tokenService.CreateToken(user).Result
                });
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar realizar Login. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Realizar atualização de usuário
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <remarks>
        /// Exemplo de requisição
        /// 
        ///      PUT /api/Account/UpdateUser
        ///      {
        ///          "titulo": 0,
        ///          "userName": "Nome de usuário",
        ///          "primeiroNome": "Nome do usuário",
        ///          "ultimoNome": "Sobrenome do usuário",
        ///          "email": "Email do usuário",
        ///          "phoneNumber": "Telefone do usuário",
        ///          "funcao": 0,
        ///          "descricao": "Descrição do usuário",
        ///          "password": "Senha do usuário"
        ///      }
        /// </remarks>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpPut("UpdateUser")]
        [ProducesResponseType(typeof(SucessRetorno<UserUpdateDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> UpdateUser(UserUpdateDto userUpdateDto)
        {
            try
            {
                if (userUpdateDto.UserName != User.GetUserName())
                    return Unauthorized("Usuário Inválido");

                var user = await _accountService.GetUserByUserNameAsync(User.GetUserName());
                if (user == null) return Unauthorized("Usuário Inválido");

                var userReturn = await _accountService.UpdateAccount(userUpdateDto);
                if (userReturn == null) return NoContent();

                return Ok(new
                {
                    userName = userReturn.UserName,
                    PrimeroNome = userReturn.PrimeiroNome,
                    token = _tokenService.CreateToken(userReturn).Result
                });
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar Atualizar Usuário. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Realizar atualização da imagem do usuário
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpPost("upload-image")]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> UploadImage()
        {
            try
            {
                var user = await _accountService.GetUserByUserNameAsync(User.GetUserName());
                if (user == null) return NoContent();

                var file = Request.Form.Files[0];
                if (file.Length > 0)
                {
                    _util.DeleteImage(user.ImagemURL, _destino);
                    user.ImagemURL = await _util.SaveImage(file, _destino);
                }
                var userRetorno = await _accountService.UpdateAccount(user);

                return Ok(userRetorno);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar realizar upload de Foto do Usuário! Erro: {ex.Message}");
            }
        }
    }
}