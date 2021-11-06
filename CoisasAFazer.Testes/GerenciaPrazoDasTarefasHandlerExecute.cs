using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CoisasAFazer.Testes
{
    public class GerenciaPrazoDasTarefasHandlerExecute
    {
        [Fact]
        public void QuandoTarefasEstiveremAtrasadasDeveMudarSeuStatus()
        {
            // arrange
            var compraCategoria = new Categoria(1, "Compra");
            var casaCategoria = new Categoria(2, "Casa");

            var tarefas = new List<Tarefa>
            {
                // atrasadas
                new Tarefa(1, "Tirar lixo", casaCategoria, new DateTime(2019, 11, 23), null, StatusTarefa.Criada),
                new Tarefa(2, "Lavar Louça", casaCategoria, new DateTime(2019, 11, 21), null, StatusTarefa.Pendente),
                new Tarefa(4, "Lavar Louça", casaCategoria, new DateTime(2019, 11, 21), null, StatusTarefa.Criada),
                new Tarefa(3, "Comprar sal", compraCategoria, new DateTime(2019, 11, 24), null, StatusTarefa.Criada)
            };

            var options = new DbContextOptionsBuilder<DbTarefasContext>()
                .UseInMemoryDatabase("DbTarefasContext")
                .Options;

            var contexto = new DbTarefasContext(options);
            var repo = new RepositorioTarefa(contexto);

            repo.IncluirTarefas(tarefas.ToArray());

            var comando = new GerenciaPrazoDasTarefas(new DateTime(2019, 11, 22));
            var handler = new GerenciaPrazoDasTarefasHandler(repo);

            // act
            handler.Execute(comando);

            // assert
            var tarefasEmAtraso = repo.ObtemTarefas(t => t.Status == StatusTarefa.EmAtraso);
            Assert.Equal(2, tarefasEmAtraso.Count());
        }

        [Fact]
        public void QuandoInvocadoDeveChamarAtualizarTarefasNaQtdeDeVezesDoTotalDeTarefasAtrasadas()
        {
            // arrange
            var casaCategoria = new Categoria(2, "Casa");

            var tarefas = new List<Tarefa>
            {
                // atrasadas
                new Tarefa(1, "Tirar lixo", casaCategoria, new DateTime(2019, 11, 23), null, StatusTarefa.Criada),
                new Tarefa(2, "Lavar Louça", casaCategoria, new DateTime(2019, 11, 21), null, StatusTarefa.Pendente),
                new Tarefa(4, "Lavar Louça", casaCategoria, new DateTime(2019, 11, 21), null, StatusTarefa.Criada),
            };

            var mock = new Mock<IRepositorioTarefas>();
            
            mock.Setup(r => r.ObtemTarefas(It.IsAny<Func<Tarefa, bool>>()))
                .Returns(tarefas);

            var repo = mock.Object;
            var comando = new GerenciaPrazoDasTarefas(new DateTime(2019, 11, 22));
            var handler = new GerenciaPrazoDasTarefasHandler(repo);

            // act
            handler.Execute(comando);

            // assert
            mock.Verify(r => r.AtualizarTarefas(It.IsAny<Tarefa[]>()), Times.Once());
        }
    }
}
