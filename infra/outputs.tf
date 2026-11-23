output "public_ip" {
  description = "The public IP address of the ShortLink instance"
  value       = aws_instance.shortlink.public_ip
}

output "ssh_command" {
  description = "The SSH command to connect to the ShortLink instance"
  value       = "ssh -i ${trimsuffix(var.ssh_public_key_path, ".pub")} ubuntu@${aws_instance.shortlink.public_ip}"
}