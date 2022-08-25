using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using static System.Net.WebRequestMethods;
using System.Xml.Linq;

namespace WebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PokemonController : ControllerBase
    {

        private readonly DataContext _context;

        public PokemonController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Pokemon>>> Get()
        {

            using var reader = new StreamReader(".\\data.json");
            var json = await reader.ReadToEndAsync();
            var data = JsonSerializer.Deserialize<List<Pokemon>>(json);

            //return Ok(await _context.Pokemons.ToListAsync());
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pokemon>> Get(int id)
        {

            //var pokemon = await _context.Pokemons.ToListAsync();

            using var reader = new StreamReader(".\\data.json");
            var json = await reader.ReadToEndAsync();
            var data = JsonSerializer.Deserialize<List<Pokemon>>(json);

            var result = data.Where(x => x.Id == id);

            //var pokemon = await _context.Pokemons.FindAsync(id);
            
            if (result == null)
                return BadRequest("Pokémon não encontrado");
            
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<List<Pokemon>>> AddPokemon([FromBody]Pokemon pkm)
        {
            var reader = new StreamReader(".\\data.json");
            var json = reader.ReadToEnd();
            reader.Dispose();
            var data = JsonSerializer.Deserialize<List<Pokemon>>(json);

            data.Add(pkm);
            var content = JsonSerializer.Serialize(data);
            System.IO.File.WriteAllText(".\\data.json", content);
            //await _context.SaveChangesAsync();
            return Ok(data);
        }

        [HttpPut]
        public async Task<ActionResult<List<Pokemon>>> UpdatePokemon(Pokemon request)
        {

            var reader = new StreamReader(".\\data.json");
            var json = reader.ReadToEnd();
            reader.Dispose();
            var data = JsonSerializer.Deserialize<List<Pokemon>>(json);

            var dbPokemon = await _context.Pokemons.FindAsync(request.Id);

            var pokeUpdate = data.Where(x => x.Id == request.Id).First();

            data.Remove(pokeUpdate);

            var changedPokemon = new Pokemon
            {
                Id = request.Id,
                Name = request.Name,
                Type = request.Type,
                Region = request.Region,
                Hp = request.Hp,
                Attack = request.Attack
            };

            data.Add(changedPokemon);
            var content = JsonSerializer.Serialize(data);
            System.IO.File.WriteAllText(".\\data.json", content);

            return Ok(data);
        }


        [HttpDelete("remove")]
        public async Task<ActionResult<List<Pokemon>>> DeletePokemon([FromBody] Pokemon pkm)
        {

            var reader = new StreamReader(".\\data.json");
            var json = reader.ReadToEnd();
            reader.Dispose();
            var data = JsonSerializer.Deserialize<List<Pokemon>>(json);

            data.Remove((Pokemon)data.Where(x => x.Id == pkm.Id).First());
            var content = JsonSerializer.Serialize(data);
            System.IO.File.WriteAllText(".\\data.json", content);
            //await _context.SaveChangesAsync();
            return Ok(data);
        }
        

        //Busca Região
        [HttpGet("LinqRegion")]
        public async Task<ActionResult<List<Pokemon>>> GetPokeRegion([FromQuery]string filter, [FromQuery] string orderBy)
            {

            using var reader = new StreamReader(".\\data.json");
            var json = await reader.ReadToEndAsync();
            var data = JsonSerializer.Deserialize<List<Pokemon>>(json);
            //var pokemon = await _context.Pokemons.ToListAsync();

            var result = data.Where(x => x.Region == filter);

            var filteredData = result;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filteredData = result
                    .Where(x => x.Region == filter).ToList();
            }

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                var propertyInfo = typeof(Pokemon).GetProperty(orderBy);
                filteredData = filteredData.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
            }

            if (result == null)
            {
                return Ok(new
                {
                    message = "Pokémon não encontrado"
                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Deu certo!",
                Data = filteredData
            });
        }

        //----------------------------



        //Busca Tipo
        [HttpGet("LinqType")]
        public async Task<ActionResult<List<Pokemon>>> GetPokeType([FromQuery] string filter, [FromQuery] string orderBy)
        {

            using var reader = new StreamReader(".\\data.json");
            var json = await reader.ReadToEndAsync();
            var data = JsonSerializer.Deserialize<List<Pokemon>>(json);
            //var pokemon = await _context.Pokemons.ToListAsync();

            var result = data.Where(x => x.Type.Contains(filter));

            var filteredData = result;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filteredData = result
                    .Where(x => x.Type.Contains(filter)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                var propertyInfo = typeof(Pokemon).GetProperty(orderBy);
                filteredData = filteredData.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
            }

            if (result == null)
            {
                return Ok(new
                {
                    message = "Pokémon não encontrado"
                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Deu certo!",
                Data = filteredData
            });
        }



        //---------------------------


        [HttpGet("Battle_TesteAlfa")]
        public async Task<ActionResult<List<Pokemon>>> GetBattle([FromQuery]int id1, [FromQuery] int id2)
        {

            var poke1 = await _context.Pokemons.FindAsync(id1);
            var poke2 = await _context.Pokemons.FindAsync(id2);

            var Poke1Battlepoints = poke1.Hp + poke1.Attack;
            var Poke2Battlepoints = poke2.Hp + poke2.Attack;

            if(Poke1Battlepoints > Poke2Battlepoints) { 
                return Ok(new
                {
                    StatusCode = 200,
                    Message = $"Pokémon {poke1.Name} venceu!",
                    Data = poke1
                });
            } else if(Poke1Battlepoints < Poke2Battlepoints) {
                return Ok(new
                {
                    StatusCode = 200,
                    Message = $"Pokémon {poke2.Name} venceu!",
                    Data = poke2
                });
            } else {
                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Empatou!",
                    Data = poke1 + "\n" + poke2
                });
            }
        }
    }
}
