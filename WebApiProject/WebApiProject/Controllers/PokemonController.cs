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

            //var dbPokemon = await _context.Pokemons.FindAsync(request.Id);

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
        public async Task<ActionResult<Pokemon>> GetBattle(int id1, int id2)
        {

            //var pokemon = await _context.Pokemons.ToListAsync();

            using var reader = new StreamReader(".\\data.json");
            var json = await reader.ReadToEndAsync();
            var data = JsonSerializer.Deserialize<List<Pokemon>>(json);

            var poke1 = data.Where(x => x.Id == id1).First();
            var poke2 = data.Where(x => x.Id == id2).First();

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

        [HttpGet("Get_Status")]
        public async Task<ActionResult<List<Pokemon>>> GetHp()
        {

            using var reader = new StreamReader(".\\data.json");
            var json = await reader.ReadToEndAsync();
            var data = JsonSerializer.Deserialize<List<Pokemon>>(json);

            var totalHP = 0;
            var mediumHP = 0;

            var totalAttack = 0;
            var mediumAttack = 0;

            var totalPoke = 0;

            //MAIOR HP
            var highestHP = int.MinValue;
            var HighestHpName = "";


            //MAIOR ATAQUE
            var highestAttack = int.MinValue;
            var HighestAttackName = "";

            //LOOP
            var breakLoop = true;

            var resultHP = data.Where(x => x.Hp > totalHP).ToList();

            while (breakLoop)
            {

                foreach (var poke in resultHP)
                {
                    if (poke.Hp > highestHP)
                    {

                        highestHP = poke.Hp;
                        HighestHpName = poke.Name;

                    }
                }

                breakLoop = false;

            }

            breakLoop = true;

            var resultAttack = data.Where(x => x.Attack > totalAttack).ToList();

            while (breakLoop)
            {

                foreach (var poke in resultAttack)
                {
                    if (poke.Attack > highestAttack)
                    {

                        highestAttack = poke.Attack;
                        HighestAttackName = poke.Name;

                    }
                }

                breakLoop = false;

            }



            for (int i = 0; i < data.Count; i++)
            {
                totalPoke++;
            }

            //TOTAL DE HP
            foreach (var poke in resultHP)
            {
                totalHP = totalHP + poke.Hp;
            }

            //MÉDIA DE HP
            mediumHP = totalHP / totalPoke;

            //TOTAL DE ATAQUE
            foreach (var poke in resultAttack)
            {
                totalAttack = totalAttack + poke.Attack;
            }

            //MÉDIA DE ATAQUE
            mediumAttack = totalAttack / totalPoke;

            return Ok($"HP TOTAL = {totalHP} \nMÉDIA DE HP = {mediumHP} \nMAIOR HP = {highestHP} (Pokémon: {HighestHpName}) \n\nATAQUE TOTAL = {totalAttack} \nMÉDIA DE ATAQUE = {mediumAttack} " +
                $"\nMAIOR ATAQUE = {highestAttack} (Pokémon: {HighestAttackName})");

        }
    }
}
