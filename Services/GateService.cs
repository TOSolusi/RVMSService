using Microsoft.EntityFrameworkCore;
using RVMSService.Data;
using RVMSService.Models;
using System.Security.Cryptography;

namespace RVMSService.Services
{
    public class GateService : IGateService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<GateService> _logger;
        private readonly IAuditTrailService _auditTrail;
        public GateService(AppDBContext context, ILogger<GateService> logger, IAuditTrailService auditTrail)
        {
            _context = context;
            _logger = logger;
            _auditTrail = auditTrail;
        }
        public async Task<Guid?> AddGate(GateModel gate)
        {
            try
            {
                _logger.LogInformation("Adding gate to database");


                await _context.Gates.AddAsync(gate);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Gate added with ID: {GateId}", gate.GateId);

                //record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = "AddGate",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Gate"
                };
                await _auditTrail.RecordAsync(audit);


                // After SaveChangesAsync, gate.GateId will contain the generated GUID
                return gate.GateId;
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed

                var audit = new AuditTrailModel
                {
                    //UserId = , /* get user id from context */
                    Description = "AddGate",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Gate"
                };
                await _auditTrail.RecordAsync(audit);

                _logger.LogError(ex, "Error occurred while adding gate to database");
                throw new Exception("An error occurred while adding the gate.", ex);
            }
        }

        public async Task<List<GateModel>> GetAllGates()
        {
            try
            {
                var gates = await _context.Gates.ToListAsync();
                _logger.LogInformation("Fetched all gates, count: {Count}", gates.Count);

                // Optionally record audit trail
                var audit = new AuditTrailModel
                {
                    // UserId = /* get user id from context */,
                    Description = "GetAllGates",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Gate"
                };
                await _auditTrail.RecordAsync(audit);

                return gates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all gates");

                var audit = new AuditTrailModel
                {
                    // UserId = /* get user id from context */,
                    Description = "GetAllGates",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Gate"
                };
                await _auditTrail.RecordAsync(audit);

                throw new Exception("An error occurred while fetching gates.", ex);

            }
        }

        public async Task UpdateGate(GateModel gate)
        {
            try
            {
                var existingGate = await _context.Gates.FindAsync(gate.GateId);
                if (existingGate == null)
                {
                    throw new Exception("Gate not found");
                }

             
                existingGate.GateName = gate.GateName;
                existingGate.Description = gate.Description;
                existingGate.UpdatedAt = DateTime.UtcNow;
                existingGate.Status = gate.Status;
                // Update other properties as needed
                await _context.SaveChangesAsync();
                _logger.LogInformation("Gate updated with ID: {GateId}", gate.GateId);
                //record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Update Gate {gate.GateName} information",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Gate"
                };
                await _auditTrail.RecordAsync(audit);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                var audit = new AuditTrailModel
                {
                    //UserId = , /* get user id from context */
                    Description = $"Update Gate {gate.GateName}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Gate"
                };
                await _auditTrail.RecordAsync(audit);
                _logger.LogError(ex, "Error occurred while updating gate in database");
                throw new Exception("An error occurred while updating the gate.", ex);
            }

        }

        public async Task DeleteGate(GateModel gate)
        {
            try
            {
                var existingGate = await _context.Gates.FindAsync(gate.GateId);
                if (existingGate == null)
                {
                    throw new Exception("Gate not found");
                }
                _context.Gates.Remove(existingGate);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Gate deleted with ID: {GateId}, Name: {GateName}", gate.GateId, gate.GateName);
                //record audit trail
                var audit = new AuditTrailModel
                {
                    //UserId = /* get user id from context */
                    Description = $"Delete Gate {gate.GateName}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Gate"
                };
                await _auditTrail.RecordAsync(audit);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                var audit = new AuditTrailModel
                {
                    //UserId = , /* get user id from context */
                    Description = $"Delete Gate {gate.GateName}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Gate"
                };
                await _auditTrail.RecordAsync(audit);
                _logger.LogError(ex, "Error occurred while deleting gate from database");
                throw new Exception("An error occurred while deleting the gate.", ex);
            }
        }


        public async Task<GateModel> GetGateById(Guid gateId)
        {
            try
            {
                var gate = await _context.Gates.FindAsync(gateId);
                if (gate == null)
                {
                    throw new Exception("Gate not found");
                }
                if (gate.Status == false)
                {
                    throw new Exception("Gate not active");
                }
                _logger.LogInformation("Fetched gate with ID: {GateId}", gateId);
                // Optionally record audit trail
                var audit = new AuditTrailModel
                {
                    // UserId = /* get user id from context */,
                    Description = $"GetGateById {gateId}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Category = "Gate"
                };
                await _auditTrail.RecordAsync(audit);
                return gate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching gate with ID: {GateId}", gateId);
                var audit = new AuditTrailModel
                {
                    // UserId = /* get user id from context */,
                    Description = $"GetGateById {gateId}",
                    Timestamp = DateTime.UtcNow,
                    Status = "Failure",
                    Category = "Gate"
                };
                await _auditTrail.RecordAsync(audit);
                throw new Exception("An error occurred while fetching the gate.", ex);
            }
        }
    }

}

   