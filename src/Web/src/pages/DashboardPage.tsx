import { Typography } from "antd";
const { Title } = Typography;

export function DashboardPage() {
  return (
    <div style={{ padding: "24px" }}>
      <Title level={3} style={{ fontWeight: "bold" }}>
        Welcome to Task Management System
      </Title>
      <Title level={4} style={{ marginTop: "24px", fontWeight: "bold" }}>
        Technology Stack
      </Title>
      <ul
        style={{
          fontSize: "16px",
          lineHeight: "2",
          listStyleType: "disc",
          paddingLeft: "24px",
        }}
      >
        <li>
          <span style={{ fontWeight: "bold" }}>Backend:</span> .NET 8 with
          Microservice Architecture
        </li>
        <li>
          <span style={{ fontWeight: "bold" }}>Database:</span> Microsoft SQL
          Server
        </li>
        <li>
          <span style={{ fontWeight: "bold" }}>Communication:</span> gRPC &
          Protobuf
        </li>
        <li>
          <span style={{ fontWeight: "bold" }}>Architecture Pattern:</span> CQRS
          with MediatR
        </li>
        <li>
          <span style={{ fontWeight: "bold" }}>Message Broker:</span>{" "}
          MassTransit & RabbitMQ
        </li>
        <li>
          <span style={{ fontWeight: "bold" }}>Email Integration:</span> Gmail
          API
        </li>
        <li>
          <span style={{ fontWeight: "bold" }}>Frontend:</span> React with
          TypeScript
        </li>
        <li>
          <span style={{ fontWeight: "bold" }}>UI Framework:</span> Ant Design
        </li>
      </ul>
    </div>
  );
}
