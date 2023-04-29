using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProEventos.Application.Contratos;
using Microsoft.AspNetCore.Http;
using ProEventos.Application.Dtos;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using ProEventos.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using ProEventos.Persistence.Models;
using ProEventos.Api.Helpers;
using ProEventos.Domain;

namespace ProEventos.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EventosController : ControllerBase
    {
        private readonly IEventoService _eventoService;
        private readonly IUtil _util;
        private readonly IAccountService _accountService;

        private readonly string _destino = "Images";

        public EventosController(IEventoService eventoService,
                                 IUtil util,
                                 IAccountService accountService)
        {
            _util = util;
            _accountService = accountService;
            _eventoService = eventoService;
        }

        /// <summary>
        /// Retornar lista de eventos
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpGet]
        [ProducesResponseType(typeof(SucessRetorno<EventoDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> Get([FromQuery] PageParams pageParams)
        {
            try
            {
                var eventos = await _eventoService.GetAllEventosAsync(User.GetUserId(), pageParams, true);
                if (eventos == null) return NoContent();

                Response.AddPagination(eventos.CurrentPage, eventos.PageSize, eventos.TotalCount, eventos.TotalPages);

                return Ok(eventos);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar recuperar eventos. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Retornar informações de evento
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SucessRetorno<EventoDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var evento = await _eventoService.GetEventoByIdAsync(User.GetUserId(), id, true);
                if (evento == null) return NoContent();

                return Ok(evento);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar recuperar eventos. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Realizar atualização da imagem do evento
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpPost("upload-image/{eventoId}")]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> UploadImage(int eventoId)
        {
            try
            {
                var evento = await _eventoService.GetEventoByIdAsync(User.GetUserId(), eventoId, true);
                if (evento == null) return NoContent();

                var file = Request.Form.Files[0];
                if (file.Length > 0)
                {
                    _util.DeleteImage(evento.ImagemURL, _destino);
                    evento.ImagemURL = await _util.SaveImage(file, _destino);
                }
                var EventoRetorno = await _eventoService.UpdateEvento(User.GetUserId(), eventoId, evento);

                return Ok(EventoRetorno);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar realizar upload de foto do evento. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Realizar cadastro de um novo evento
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <remarks>
        /// Exemplo de requisição
        /// 
        ///      POST /api/Eventos
        ///      {
        ///          "local": "Local do evento",
        ///          "dataEvento": "Data do evento",
        ///          "tema": "Tema do evento",
        ///          "qtdPessoas": 0,
        ///          "telefone": "Telefone do evento",
        ///          "email": "Email do evento",
        ///          "lotes": [],
        ///          "redesSociais": []",
        ///          "palestrantes": []
        ///      }
        /// </remarks>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpPost]
        [ProducesResponseType(typeof(SucessRetorno<EventoDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> Post(EventoDto model)
        {
            try
            {
                var evento = await _eventoService.AddEventos(User.GetUserId(), model);
                if (evento == null) return NoContent();

                return Ok(evento);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar adicionar eventos. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Realizar atualização de evento
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <remarks>
        /// Exemplo de requisição
        /// 
        ///      PUT /api/Eventos/{id}
        ///      {
        ///          "local": "Local do evento",
        ///          "dataEvento": "Data do evento",
        ///          "tema": "Tema do evento",
        ///          "qtdPessoas": 0,
        ///          "telefone": "Telefone do evento",
        ///          "email": "Email do evento",
        ///          "lotes": [
        ///             {
        ///                 "nome": "Nome do lote",
        ///                 "preco": 0,
        ///                 "dataInicio": "Data de início da abertura do lote",
        ///                 "dataFim": "Data final da abertura do lote",
        ///                 "quantidade": 0,
        ///                 "eventoId": 0
        ///             }
        ///          ],
        ///          "redesSociais": [
        ///             { 
        ///                 "nome": "Nome da rede social",
        ///                 "url": "URL da rede social",
        ///                 "eventoId": 0,
        ///             }
        ///          ]",
        ///          "palestrantes": []
        ///      }
        /// </remarks>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SucessRetorno<EventoDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> Put(int id, EventoDto model)
        {
            try
            {
                var evento = await _eventoService.UpdateEvento(User.GetUserId(), id, model);
                if (evento == null) return NoContent();

                return Ok(evento);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar atualizar eventos. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Realizar exclusão do evento
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(SucessRetorno<SucessRetornoDelete>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var evento = await _eventoService.GetEventoByIdAsync(User.GetUserId(), id, true);
                if (evento == null) return NoContent();

                if (await _eventoService.DeleteEvento(User.GetUserId(), id))
                {
                    _util.DeleteImage(evento.ImagemURL, _destino);
                    return Ok(new { message = "Deletado" });
                }
                else
                {
                    throw new Exception("Ocorreu um problem não específico ao tentar deletar Evento.");
                }
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar deletar eventos. Erro: {ex.Message}");
            }
        }
    }
}
