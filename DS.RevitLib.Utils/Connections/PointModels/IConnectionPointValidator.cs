using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DS.RevitLib.Utils.Connections.PointModels.PointModels
{
    public interface IConnectionPointValidator
    {
        IEnumerable<ValidationResult> GetValidationResults(ConnectionPoint connectionPoint);
        bool Validate(ConnectionPoint connectionPoint);
    }
}