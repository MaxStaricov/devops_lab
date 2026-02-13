using Models;

namespace DTOs;


public record class TodoDto {
    public int Id {get; init;}
    public string Data {get; init;}
    public bool IsComplited {get; init;}

    public TodoDto(Todo todoModel) {
        this.Id = todoModel.Id;
        this.Data = todoModel.Data;
        this.IsComplited = todoModel.IsComplited;
    }
}