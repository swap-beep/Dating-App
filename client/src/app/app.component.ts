import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit ,inject} from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet,CommonModule ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  standalone:true
})
export class AppComponent implements OnInit {
  http=inject(HttpClient);
  title = 'Dating App';
  users:any;

  ngOnInit(): void {
    this.http.get('https://localhost:5001/api/User').subscribe({
      next: response => this.users =response,
error: error => console.log(error),
complete : () =>console.log('Request has completed')
    })
  }
}
