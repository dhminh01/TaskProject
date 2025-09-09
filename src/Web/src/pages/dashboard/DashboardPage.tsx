import { WaterMark } from "@ant-design/pro-components";
import { Typography } from "antd";
const { Title } = Typography;

export function DashboardPage() {
  return (
    <WaterMark content={["taskapp - dhminh01"]}>
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
            <span style={{ fontWeight: "bold" }}>Backend:</span>{" "}
            <a
              href="https://dotnet.microsoft.com/en-us/download/dotnet/8.0"
              target="_blank"
              rel="noopener noreferrer"
            >
              .NET 8
            </a>{" "}
            with Microservice Architecture
          </li>
          <li>
            <span style={{ fontWeight: "bold" }}>Database:</span>{" "}
            <a
              href="https://www.microsoft.com/en-us/sql-server"
              target="_blank"
              rel="noopener noreferrer"
            >
              Microsoft SQL Server
            </a>
          </li>
          <li>
            <span style={{ fontWeight: "bold" }}>Communication:</span>{" "}
            <a
              href="https://grpc.io/"
              target="_blank"
              rel="noopener noreferrer"
            >
              gRPC
            </a>{" "}
            &{" "}
            <a
              href="https://protobuf.dev/"
              target="_blank"
              rel="noopener noreferrer"
            >
              Protobuf
            </a>
          </li>
          <li>
            <span style={{ fontWeight: "bold" }}>Architecture Pattern:</span>{" "}
            <a
              href="https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs"
              target="_blank"
              rel="noopener noreferrer"
            >
              CQRS
            </a>{" "}
            with{" "}
            <a
              href="https://github.com/jbogard/MediatR"
              target="_blank"
              rel="noopener noreferrer"
            >
              MediatR
            </a>
          </li>
          <li>
            <span style={{ fontWeight: "bold" }}>Message Broker:</span>{" "}
            <a
              href="https://masstransit.io/"
              target="_blank"
              rel="noopener noreferrer"
            >
              MassTransit
            </a>{" "}
            &{" "}
            <a
              href="https://www.rabbitmq.com/"
              target="_blank"
              rel="noopener noreferrer"
            >
              RabbitMQ
            </a>
          </li>
          <li>
            <span style={{ fontWeight: "bold" }}>Email Integration:</span>{" "}
            <a
              href="https://developers.google.com/gmail/api"
              target="_blank"
              rel="noopener noreferrer"
            >
              Gmail API
            </a>
          </li>
          <li>
            <span style={{ fontWeight: "bold" }}>Frontend:</span>{" "}
            <a
              href="https://react.dev/"
              target="_blank"
              rel="noopener noreferrer"
            >
              React
            </a>{" "}
            with{" "}
            <a
              href="https://www.typescriptlang.org/"
              target="_blank"
              rel="noopener noreferrer"
            >
              TypeScript
            </a>
          </li>
          <li>
            <span style={{ fontWeight: "bold" }}>UI Framework:</span>{" "}
            <a
              href="https://ant.design/"
              target="_blank"
              rel="noopener noreferrer"
            >
              Ant Design
            </a>
          </li>
        </ul>
      </div>
    </WaterMark>
  );
}
