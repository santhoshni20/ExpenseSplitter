using ExpenseSplitter.Interfaces;
using ExpenseSplitter.Models;
using ExpenseSplitter.Models.DTOs;
using ExpenseSplitter.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ExpenseSplitter.Controllers
{
    public class HomeController : Controller
    {
        private readonly IParticipantRepository _participantRepo;
        private readonly IExpenseRepository _expenseRepo;
        private readonly ISettlementService _settlementService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IParticipantRepository participantRepo,
            IExpenseRepository expenseRepo,
            ISettlementService settlementService,
            ILogger<HomeController> logger)
        {
            _participantRepo = participantRepo;
            _expenseRepo = expenseRepo;
            _settlementService = settlementService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var participants = (await _participantRepo.GetAllAsync()).ToList();
            var expenses = (await _expenseRepo.GetAllWithDetailsAsync()).ToList();

            var balances = _settlementService.CalculateBalances(participants, expenses);
            var settlements = _settlementService.CalculateMinSettlements(balances);

            var expenseDtos = expenses.Select(e => new ExpenseItemDto
            {
                Id = e.Id,
                Description = e.Description,
                Amount = e.Amount,
                Date = e.Date,
                PayerId = e.PayerId,
                PayerName = e.Payer?.Name ?? "Unknown",
                SharedByNames = e.ExpenseShares.Select(s => s.Participant?.Name ?? "Unknown").ToList(),
                SharePerPerson = e.ExpenseShares.Count > 0 ? e.Amount / e.ExpenseShares.Count : 0
            }).ToList();

            var participantDtos = participants.Select(p => new ParticipantDto
            {
                Id = p.Id,
                Name = p.Name
            }).ToList();

            var viewModel = new DashboardViewModel
            {
                Participants = participantDtos,
                Expenses = expenseDtos,
                Balances = balances,
                Settlements = settlements,
                TotalExpenses = expenses.Sum(e => e.Amount)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddParticipant(ParticipantDto newParticipant)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage
                            ?? "Invalid participant name.";
                TempData["Error"] = error;
                return RedirectToAction(nameof(Index));
            }

            if (await _participantRepo.ExistsByNameAsync(newParticipant.Name))
            {
                TempData["Error"] = $"Participant '{newParticipant.Name.Trim()}' already exists.";
                return RedirectToAction(nameof(Index));
            }

            var participant = new Participant
            {
                Name = newParticipant.Name.Trim()
            };

            await _participantRepo.AddAsync(participant);
            TempData["Success"] = $"Participant '{participant.Name}' added successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteParticipant(int id)
        {
            bool deleted = await _participantRepo.DeleteAsync(id);
            if (deleted)
            {
                TempData["Success"] = "Participant removed successfully.";
            }
            else
            {
                TempData["Error"] = "Cannot delete participant who is associated with existing expenses or shares.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExpense(CreateExpenseDto newExpense)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage
                            ?? "Please correct the form errors.";
                TempData["Error"] = error;
                return RedirectToAction(nameof(Index));
            }

            if (newExpense.ParticipantIds == null || !newExpense.ParticipantIds.Any())
            {
                TempData["Error"] = "Please select at least one participant sharing the expense.";
                return RedirectToAction(nameof(Index));
            }

            var payer = await _participantRepo.GetByIdAsync(newExpense.PayerId);
            if (payer == null)
            {
                TempData["Error"] = "Selected payer does not exist.";
                return RedirectToAction(nameof(Index));
            }

            var expense = new Expense
            {
                Description = newExpense.Description.Trim(),
                Amount = newExpense.Amount,
                PayerId = newExpense.PayerId
            };

            await _expenseRepo.AddAsync(expense, newExpense.ParticipantIds);
            TempData["Success"] = $"Expense '{expense.Description}' of {expense.Amount:C2} added and split equally among {newExpense.ParticipantIds.Count} person(s).";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            bool deleted = await _expenseRepo.DeleteAsync(id);
            if (deleted)
            {
                TempData["Success"] = "Expense deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Expense not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
