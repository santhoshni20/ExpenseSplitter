using ExpenseSplitter.Models.DTOs;
using ExpenseSplitter.Models.Entities;
using ExpenseSplitter.Repositories;
using Xunit;

namespace ExpenseSplitter.Tests
{
    public class SettlementServiceTests
    {
        private readonly SettlementService _service = new SettlementService();

        [Fact]
        public void CalculateBalances_SingleExpense_CalculatesCorrectNetBalances()
        {
            // Arrange: Alice, Bob, Charlie
            var alice = new Participant { Id = 1, Name = "Alice" };
            var bob = new Participant { Id = 2, Name = "Bob" };
            var charlie = new Participant { Id = 3, Name = "Charlie" };

            var participants = new List<Participant> { alice, bob, charlie };

            // Alice pays $90 for dinner shared by Alice, Bob, Charlie ($30 each)
            var expense = new Expense
            {
                Id = 1,
                Description = "Dinner",
                Amount = 90.00m,
                PayerId = 1,
                ExpenseShares = new List<ExpenseShare>
                {
                    new ExpenseShare { ParticipantId = 1, ShareAmount = 30.00m },
                    new ExpenseShare { ParticipantId = 2, ShareAmount = 30.00m },
                    new ExpenseShare { ParticipantId = 3, ShareAmount = 30.00m }
                }
            };

            // Act
            var balances = _service.CalculateBalances(participants, new List<Expense> { expense });

            // Assert
            var aliceBal = balances.First(b => b.ParticipantId == 1);
            var bobBal = balances.First(b => b.ParticipantId == 2);
            var charlieBal = balances.First(b => b.ParticipantId == 3);

            Assert.Equal(90.00m, aliceBal.TotalPaid);
            Assert.Equal(30.00m, aliceBal.TotalOwed);
            Assert.Equal(60.00m, aliceBal.NetBalance);
            Assert.Equal("Receives", aliceBal.Status);

            Assert.Equal(0.00m, bobBal.TotalPaid);
            Assert.Equal(30.00m, bobBal.TotalOwed);
            Assert.Equal(-30.00m, bobBal.NetBalance);
            Assert.Equal("Owes", bobBal.Status);

            Assert.Equal(0.00m, charlieBal.TotalPaid);
            Assert.Equal(30.00m, charlieBal.TotalOwed);
            Assert.Equal(-30.00m, charlieBal.NetBalance);
            Assert.Equal("Owes", charlieBal.Status);
        }

        [Fact]
        public void CalculateMinSettlements_SingleExpense_ProducesMinimalDirectTransactions()
        {
            // Arrange
            var balances = new List<ParticipantBalanceDto>
            {
                new ParticipantBalanceDto { ParticipantId = 1, ParticipantName = "Alice", NetBalance = 60.00m },
                new ParticipantBalanceDto { ParticipantId = 2, ParticipantName = "Bob", NetBalance = -30.00m },
                new ParticipantBalanceDto { ParticipantId = 3, ParticipantName = "Charlie", NetBalance = -30.00m }
            };

            // Act
            var settlements = _service.CalculateMinSettlements(balances);

            // Assert
            Assert.Equal(2, settlements.Count);
            Assert.All(settlements, s => Assert.Equal("Alice", s.ToParticipantName));
            Assert.Contains(settlements, s => s.FromParticipantName == "Bob" && s.Amount == 30.00m);
            Assert.Contains(settlements, s => s.FromParticipantName == "Charlie" && s.Amount == 30.00m);
        }

        [Fact]
        public void CalculateBalancesAndSettlements_MultipleExpenses_ComplexSettlement()
        {
            // Arrange:
            // Alice paid $90 for Dinner (Alice, Bob, Charlie: $30 each).
            // Bob paid $30 for Snacks (Bob, Charlie: $15 each).
            // Charlie paid $60 for Transport (Alice, Bob: $30 each).
            var alice = new Participant { Id = 1, Name = "Alice" };
            var bob = new Participant { Id = 2, Name = "Bob" };
            var charlie = new Participant { Id = 3, Name = "Charlie" };

            var participants = new List<Participant> { alice, bob, charlie };

            var expenses = new List<Expense>
            {
                new Expense
                {
                    Id = 1,
                    Description = "Dinner",
                    Amount = 90.00m,
                    PayerId = 1,
                    ExpenseShares = new List<ExpenseShare>
                    {
                        new ExpenseShare { ParticipantId = 1, ShareAmount = 30.00m },
                        new ExpenseShare { ParticipantId = 2, ShareAmount = 30.00m },
                        new ExpenseShare { ParticipantId = 3, ShareAmount = 30.00m }
                    }
                },
                new Expense
                {
                    Id = 2,
                    Description = "Snacks",
                    Amount = 30.00m,
                    PayerId = 2,
                    ExpenseShares = new List<ExpenseShare>
                    {
                        new ExpenseShare { ParticipantId = 2, ShareAmount = 15.00m },
                        new ExpenseShare { ParticipantId = 3, ShareAmount = 15.00m }
                    }
                },
                new Expense
                {
                    Id = 3,
                    Description = "Transport",
                    Amount = 60.00m,
                    PayerId = 3,
                    ExpenseShares = new List<ExpenseShare>
                    {
                        new ExpenseShare { ParticipantId = 1, ShareAmount = 30.00m },
                        new ExpenseShare { ParticipantId = 2, ShareAmount = 30.00m }
                    }
                }
            };

            // Act
            var balances = _service.CalculateBalances(participants, expenses);
            var settlements = _service.CalculateMinSettlements(balances);

            // Alice: Paid 90, owes 60 (Dinner 30 + Transport 30) -> Net = +30 (Receives 30)
            // Bob: Paid 30, owes 75 (Dinner 30 + Snacks 15 + Transport 30) -> Net = -45 (Owes 45)
            // Charlie: Paid 60, owes 45 (Dinner 30 + Snacks 15) -> Net = +15 (Receives 15)
            var aliceBal = balances.First(b => b.ParticipantId == 1);
            var bobBal = balances.First(b => b.ParticipantId == 2);
            var charlieBal = balances.First(b => b.ParticipantId == 3);

            Assert.Equal(30.00m, aliceBal.NetBalance);
            Assert.Equal(-45.00m, bobBal.NetBalance);
            Assert.Equal(15.00m, charlieBal.NetBalance);

            // Minimum payments needed:
            // Bob owes 45 -> Bob pays Alice 30, Bob pays Charlie 15. Total 2 payments.
            Assert.Equal(2, settlements.Count);
            Assert.Contains(settlements, s => s.FromParticipantName == "Bob" && s.ToParticipantName == "Alice" && s.Amount == 30.00m);
            Assert.Contains(settlements, s => s.FromParticipantName == "Bob" && s.ToParticipantName == "Charlie" && s.Amount == 15.00m);
        }

        [Fact]
        public void CalculateMinSettlements_WhenEveryoneSettled_ReturnsEmpty()
        {
            var balances = new List<ParticipantBalanceDto>
            {
                new ParticipantBalanceDto { ParticipantId = 1, ParticipantName = "Alice", NetBalance = 0.00m },
                new ParticipantBalanceDto { ParticipantId = 2, ParticipantName = "Bob", NetBalance = 0.00m }
            };

            var settlements = _service.CalculateMinSettlements(balances);
            Assert.Empty(settlements);
        }
    }
}
